<template>
    <view class="container">
        <!-- Navigation Bar -->
        <view class="custom-nav" :style="{ paddingTop: statusBarHeight * 2 + 'rpx' }">
            <view class="nav-content">
                <view class="nav-left">
                    <BackButton class="nav-back-btn" @click="goBack" />
                </view>
                <text class="nav-title">{{ $t('rescue.title') }}</text>
                <view class="nav-right"></view>
            </view>
        </view>

        <!-- Tabs Section -->
        <CapsuleTabs :tabs="rescueTabs" v-model:current="currentTab"
            :style="{ top: (statusBarHeight * 2 + 88) + 'rpx', position: 'fixed', left: 0, width: '100%', zIndex: 998 }"
            container-style="padding: 16rpx 32rpx; border-bottom: 2rpx solid #f0f0f0;" />

        <scroll-view class="form-content" scroll-y :style="{ marginTop: (statusBarHeight * 2 + 208) + 'rpx' }"
            :refresher-enabled="true" :refresher-triggered="isRefreshing" :refresher-threshold="60"
            @refresherrefresh="onRefresh" @refresherpulling="onPulling" @refresherrestore="onRestore"
            @refresherabort="onRestore" refresher-default-style="none" refresher-background="#f5f7fa">

            <view slot="refresher" class="custom-refresher"
                :style="{ transform: (isPulling || isRefreshing) ? 'translateY(120rpx)' : 'translateY(0)' }">
                <view class="refresher-content">
                    <view class="refresher-icon" :class="{ 'animating': isRefreshing }"></view>
                    <text class="refresher-text">{{ isRefreshing ? $t('index.refreshing') : $t('index.pullToRefresh')
                        }}</text>
                </view>
            </view>
            <view class="form-card" v-if="currentTab === 0">
                <!-- Basic Info -->
                <view class="form-item">
                    <text class="label required">{{ $t('rescue.patient_name') }}</text>
                    <input class="input" v-model="formData.Name" :placeholder="$t('rescue.patient_name_placeholder')"
                        :disabled="isReadOnly" />
                </view>
                <view class="form-item">
                    <text class="label">{{ $t('rescue.gender') }}</text>
                    <view class="radio-group">
                        <view class="radio-item" @click="!isReadOnly && (formData.Gender = 1)"
                            :class="{ active: formData.Gender === 1 }">{{ $t('profile.male') }}
                        </view>
                        <view class="radio-item" @click="!isReadOnly && (formData.Gender = 2)"
                            :class="{ active: formData.Gender === 2 }">{{ $t('profile.female') }}
                        </view>
                    </view>
                </view>
                <view class="form-item">
                    <text class="label">{{ $t('rescue.age') }}</text>
                    <input class="input" type="number" v-model.number="formData.Age" :placeholder="$t('rescue.age')"
                        :disabled="isReadOnly" />
                </view>
                <view class="form-item">
                    <text class="label">{{ $t('rescue.patient_id') }}</text>
                    <input class="input" v-model="formData.PatientId" :placeholder="$t('rescue.patient_id_placeholder')"
                        :disabled="isReadOnly" />
                </view>
                <view class="form-item">
                    <text class="label">{{ $t('rescue.bed_no') }}</text>
                    <input class="input" v-model="formData.BedNO" :placeholder="$t('rescue.bed_no_placeholder')"
                        :disabled="isReadOnly" />
                </view>
                <view class="form-item">
                    <text class="label">{{ $t('rescue.department') }}</text>
                    <view class="input disabled-text">{{ formData.DepartmentName || $t('rescue.not_set') }}</view>
                </view>
                <view class="form-item">
                    <text class="label required">{{ $t('rescue.rescue_car') }}</text>
                    <view class="input disabled-text">{{ formData.CartName || $t('rescue.not_selected_car') }}</view>
                </view>
            </view>

            <view class="form-card" v-if="currentTab === 1">
                <!-- Rescue Info -->
                <view class="form-item">
                    <text class="label">{{ $t('rescue.start_time') }}</text>
                    <picker mode="date" @change="onDateChange" :disabled="isReadOnly">
                        <view class="input">{{ dateStr || $t('rescue.select_date') }}</view>
                    </picker>
                    <picker mode="time" @change="onTimeChange" :disabled="isReadOnly">
                        <view class="input" style="margin-left: 20rpx;">{{ timeStr || $t('rescue.select_time') }}</view>
                    </picker>
                </view>
                <view class="form-item column">
                    <text class="label">{{ $t('rescue.changes') }}</text>
                    <textarea class="textarea" v-model="formData.Changes"
                        :placeholder="$t('rescue.changes_placeholder')" auto-height :disabled="isReadOnly"></textarea>
                </view>
                <view class="form-item column">
                    <text class="label">{{ $t('rescue.check') }}</text>
                    <textarea class="textarea" v-model="formData.Check" :placeholder="$t('rescue.check_placeholder')"
                        auto-height :disabled="isReadOnly"></textarea>
                </view>
                <view class="form-item column">
                    <text class="label">{{ $t('rescue.diagnosis') }}</text>
                    <textarea class="textarea" v-model="formData.Diagnosis" :placeholder="$t('rescue.diagnosis_result')"
                        auto-height :disabled="isReadOnly"></textarea>
                </view>
                <view class="form-item column">
                    <text class="label">{{ $t('rescue.process') }}</text>
                    <textarea class="textarea" v-model="formData.Process" :placeholder="$t('rescue.process_record')"
                        auto-height :disabled="isReadOnly"></textarea>
                </view>
                <view class="form-item">
                    <text class="label">{{ $t('rescue.effect') }}</text>
                    <input class="input" v-model="formData.Effect" :placeholder="$t('rescue.effect')"
                        :disabled="isReadOnly" />
                </view>
                <view class="form-item column">
                    <text class="label">{{ $t('rescue.remark') }}</text>
                    <textarea class="textarea" v-model="formData.Remark" :placeholder="$t('rescue.remark_placeholder')"
                        auto-height :disabled="isReadOnly"></textarea>
                </view>
            </view>

            <view class="form-card" v-if="currentTab === 2">
                <view class="list-header" v-if="!isReadOnly">
                    <button class="add-btn" @click="openMemberSelector">
                        <text class="plus">+</text> {{ $t('rescue.add_staff') }}
                    </button>
                </view>
                <view class="staff-list">
                    <view class="staff-item" v-for="(staff, idx) in staffList" :key="idx">
                        <view class="staff-info">
                            <text class="staff-name">{{ staff.UserName }}</text>
                            <text class="staff-role" @click="!isReadOnly && changeStaffRole(staff)">{{
                                getCategoryLabel(staff.Category)
                                }}</text>
                        </view>
                        <text class="delete-btn" v-if="!isReadOnly" @click="removeStaff(staff)">{{ $t('common.delete')
                            }}</text>
                    </view>
                    <view class="empty-text" v-if="staffList.length === 0">{{ $t('rescue.no_staff') }}</view>
                </view>
            </view>

            <view class="form-card" v-if="currentTab === 3">
                <view class="medicine-list">
                    <view class="medicine-item" v-for="(item, idx) in medicineList" :key="idx">
                        <view class="medicine-info">
                            <text class="medicine-name">{{ item.ItemName }}</text>
                            <text class="medicine-spec" v-if="item.Specification">{{ item.Specification }}</text>
                        </view>
                        <view class="medicine-qty">
                            <text class="qty-num">{{ Math.abs(item.Qty) }}</text>
                        </view>
                    </view>
                    <view class="empty-text" v-if="medicineList.length === 0">{{ $t('rescue.no_medicine') }}</view>
                </view>
            </view>

            <view class="submit-section" v-if="currentTab === 1 && !isReadOnly">
                <button class="submit-btn" :disabled="!isValid" @click="submitForm">{{ $t('rescue.submit_record')
                    }}</button>
            </view>
            <view class="submit-section" v-if="currentTab === 0 && !isReadOnly">
                <button class="submit-btn" @click="currentTab = 1">{{ $t('rescue.next_step') }}</button>
            </view>
        </scroll-view>
        <!-- Member Selector Component -->
        <MemberSelector v-model:visible="showMemberSelector" @select="selectMember" :exclude-ids="joinedMemberIds" />
    </view>
</template>

<script>
import BackButton from '@/components/BackButton/BackButton.vue';
import CapsuleTabs from '@/components/CapsuleTabs/CapsuleTabs.vue';
import MemberSelector from '@/components/MemberSelector/MemberSelector.vue';
import request from '@/utils/request.js';
import { storage } from '@/utils/storage.js';
import dayjs from 'dayjs';

export default {
    components: { BackButton, CapsuleTabs, MemberSelector },
    data() {
        return {
            statusBarHeight: 20,
            isRefreshing: false,
            isPulling: false,
            formData: {
                Name: '',
                Gender: 1, // 1: Male, 2: Female
                Age: null,
                DepartmentName: '',
                BedNO: '',
                PatientId: '',
                StartTime: '',
                Changes: '',
                Check: '',
                Diagnosis: '',
                Process: '',
                Effect: '',
                Remark: '',
                Status: '',
                CartId: 0,
                CartName: ''
            },
            dateStr: '',
            timeStr: '',
            cartList: [],
            userInfo: {},
            currentTab: 0,
            rescueTabs: [
                { key: 0, label: 'rescue.tabs.basic' },
                { key: 1, label: 'rescue.tabs.rescue' },
                { key: 2, label: 'rescue.tabs.staff' },
                { key: 3, label: 'rescue.tabs.medicine' }
            ],
            // Member Selector State
            showMemberSelector: false,
            staffList: [],
            staffCategories: ['doctor', 'nurse', 'anesthetist', 'driver', 'other'],
            medicineList: []
        };
    },
    computed: {
        isValid() {
            return this.formData.Name && this.formData.CartId;
        },
        cartIndex() {
            // Use loose comparison
            return this.cartList.findIndex(c => c.Id == this.formData.CartId);
        },
        joinedMemberIds() {
            return this.staffList.map(s => s.UserId);
        },
        isReadOnly() {
            return this.formData.Status === 10 || this.formData.Status === 100;
        }
    },
    watch: {
        currentTab(newVal) {
            if (newVal === 3 && this.medicineList.length === 0) {
                this.fetchMedicineList();
            }
        }
    },
    onLoad(options) {
        const sysInfo = uni.getSystemInfoSync();
        this.statusBarHeight = sysInfo.statusBarHeight || 20;

        const uInfo = storage.get('userOrgInfo');
        if (uInfo && uInfo.User) {
            this.userInfo = uInfo.User;
            // Pre-fill department if available
            if (uInfo.Department && uInfo.Department.Name) {
                this.formData.DepartmentName = uInfo.Department.Name;
            }
        }

        // Set default time to now
        const now = dayjs();
        this.dateStr = now.format('YYYY-MM-DD');
        this.timeStr = now.format('HH:mm');
        this.updateStartTime();

        this.fetchRescueCars();

        // If ID is provided, read from cache (prioritizing the passed taskId)
        const id = options.id;
        const task = storage.get('currentRescueTask');
        if (id && task && String(task.Id) === String(id)) {
            // Edit existing record from list
            this.formData = { ...this.formData, ...task };
            if (this.formData.StartTime) {
                const start = dayjs(this.formData.StartTime);
                this.dateStr = start.format('YYYY-MM-DD');
                this.timeStr = start.format('HH:mm');
            }
        } else if (task && task.Status === 0) {
            // Entering from main page "Ongoing Rescue"
            this.formData = { ...this.formData, ...task };
            if (this.formData.StartTime) {
                const start = dayjs(this.formData.StartTime);
                this.dateStr = start.format('YYYY-MM-DD');
                this.timeStr = start.format('HH:mm');
            }
        }
        // Initialize staff list
        this.fetchStaffList();
    },
    methods: {
        goBack() {
            uni.navigateBack();
        },
        onDateChange(e) {
            this.dateStr = e.detail.value;
            this.updateStartTime();
        },
        onPulling() {
            this.isPulling = true;
        },
        onRestore() {
            this.isPulling = false;
        },
        onRefresh() {
            this.isRefreshing = true;
            this.isPulling = true;

            // Refresh logic: re-fetch staff list and info
            const p1 = this.fetchStaffList();
            const p2 = this.fetchRescueCars();

            Promise.all([p1, p2]).finally(() => {
                setTimeout(() => {
                    this.isRefreshing = false;
                    this.isPulling = false;
                }, 500);
            });
        },
        onTimeChange(e) {
            this.timeStr = e.detail.value;
            this.updateStartTime();
        },
        updateStartTime() {
            this.formData.StartTime = `${this.dateStr}T${this.timeStr}:00`;
        },
        fetchRescueCars() {
            // Get currently selected car from storage (Read-only mode)
            const carInfo = storage.get('currentRescueCarInfo');
            if (carInfo) {
                this.formData.CartId = carInfo.Id;
                this.formData.CartName = carInfo.Name || carInfo.DisplayName;
            } else {
                // Fallback if cache is missing (though index.vue should have set it)
                const lastId = storage.get('lastSelectedRescueCarId');
                if (lastId) {
                    // Original fallback logic if needed, but user requested direct cache usage.
                    // Leaving simpler logic for now.
                }
            }
        },
        submitForm() {
            if (!this.isValid) return;

            // Prepare validation or formatting if needed
            const payload = { ...this.formData };

            // If editing existing, use Update? Assuming Add/Update logic handled or this is only for New.
            // Actually, rescue-record could be Edit. If Id exists -> Update.
            const url = this.formData.Id ? 'api/ssbh/Rescue/Update' : 'api/ssbh/Rescue/Add';

            request.post(url, { mainData: payload })
                .then(res => {
                    if (res.success || res.status) {
                        uni.showToast({ title: this.$t('common.success'), icon: 'success' });
                        // If new, update Id to allow adding staff
                        if (!this.formData.Id && res.data) {
                            const data = typeof res.data === 'string' ? JSON.parse(res.data) : res.data;
                            // CAREFUL: Response format varies. Assuming standard wrapped data.
                            // if(data && data.data) this.formData.Id = data.data.Id;
                        }

                        setTimeout(() => {
                            uni.navigateBack();
                        }, 1500);
                    } else {
                        uni.showToast({ title: res.msg || this.$t('common.fail'), icon: 'none' });
                    }
                })
                .catch(e => {
                    console.error(e);
                    uni.showToast({ title: this.$t('common.fail'), icon: 'none' });
                });
        },

        // Staff Logic
        fetchStaffList() {
            if (!this.formData.Id) return Promise.resolve();
            return request.post('api/ssbh/RescueStaff/GetPageData', {
                sort: 'CreationTime',
                order: 'asc',
                wheres: JSON.stringify([{ name: 'RescueId', value: this.formData.Id }])
            }).then(res => {
                this.staffList = res.rows || [];
            });
        },

        openMemberSelector() {
            if (!this.formData.Id) {
                uni.showToast({ title: this.$t('rescue.save_first'), icon: 'none' });
                return;
            }
            this.showMemberSelector = true;
        },

        selectMember(m) {
            const payload = {
                RescueId: this.formData.Id,
                UserId: m.value,
                UserName: m.label.split(' (')[0], // Extract name without dept
                Category: 0, // Default to 0 (Doctor)
                JoinTime: dayjs().format('YYYY-MM-DD HH:mm:ss')
            };
            request.post('api/ssbh/RescueStaff/Add', { mainData: payload }).then(res => {
                if (res.success || res.status) {
                    this.fetchStaffList();
                } else {
                    uni.showToast({ title: res.msg || this.$t('common.fail'), icon: 'none' });
                }
            });
        },
        removeStaff(staff) {
            uni.showModal({
                title: this.$t('common.confirm'),
                content: this.$t('rescue.remove_staff_confirm', { name: staff.UserName }),
                success: (res) => {
                    if (res.confirm) {
                        request.post('api/ssbh/RescueStaff/Del', [staff.Id]).then(r => {
                            if (r.success || r.status) this.fetchStaffList();
                        });
                    }
                }
            });
        },
        changeStaffRole(staff) {
            uni.showActionSheet({
                itemList: this.staffCategories,
                success: (res) => {
                    const newCategory = res.tapIndex;
                    request.post('api/ssbh/RescueStaff/Update', {
                        mainData: { ...staff, Category: newCategory }
                    }).then(r => {
                        if (r.success || r.status) this.fetchStaffList();
                    });
                }
            });
        },
        getCategoryLabel(category) {
            const key = this.staffCategories[category];
            if (!key) return this.$t('rescue.set_role');
            return this.$t('rescue.staff_category.' + key);
        },
        addMedicine() {
            uni.showToast({ title: this.$t('common.coming_soon'), icon: 'none' });
        },
        fetchMedicineList() {
            if (!this.formData.Id) return;
            request.post('api/ssbh/RescueItem/GetPageData', {
                sort: 'CreationTime',
                order: 'asc',
                wheres: JSON.stringify([{ name: 'RescueId', value: this.formData.Id }])
            }).then(res => {
                this.medicineList = res.rows || [];
            });
        }
    }
}
</script>

<style lang="scss" scoped>
.container {
    background-color: $sull-color-background;
    min-height: 100vh;
    display: flex;
    flex-direction: column;
}

.custom-nav {
    background-color: $sull-color-white;
    position: fixed;
    top: 0;
    left: 0;
    width: 100%;
    z-index: 999;
    box-shadow: 0 2rpx 8rpx rgba(0, 0, 0, 0.03);

    .nav-content {
        height: 88rpx;
        display: flex;
        align-items: center;
        justify-content: space-between;
        padding: 0 32rpx;

        .nav-left,
        .nav-right {
            width: 80rpx;
        }

        .nav-title {
            font-size: 34rpx;
            font-weight: 600;
            color: #333;
        }
    }
}

.form-content {
    flex: 1;
    padding: 32rpx;
    width: 100%;
    box-sizing: border-box;
}

.form-card {
    background: #fff;
    border-radius: 24rpx;
    padding: 0 32rpx;
    margin-bottom: 48rpx;
}

.section-title {
    font-size: 32rpx;
    font-weight: bold;
    color: $sull-text-main;
    padding: 32rpx 0 16rpx 0;
    border-bottom: 2rpx solid #f5f5f5;
    margin-bottom: 16rpx;
}

.form-item {
    display: flex;
    align-items: center;
    padding: 24rpx 0;
    border-bottom: 2rpx solid #eee;

    &:last-child {
        border-bottom: none;
    }

    &.column {
        flex-direction: column;
        align-items: flex-start;
    }
}

.label {
    width: 160rpx;
    font-size: 30rpx;
    color: #333;

    &.required::after {
        content: '*';
        color: #ff4d4f;
        margin-left: 4rpx;
    }
}

.input {
    flex: 1;
    font-size: 30rpx;
    color: #333;

    &.disabled-text {
        color: #999;
    }
}

.textarea {
    width: 100%;
    min-height: 60rpx;
    font-size: 30rpx;
    line-height: 40rpx;
    color: #333;
    margin-top: 16rpx;
    background: #f9f9f9;
    padding: 10rpx 16rpx;
    border-radius: 12rpx;
    box-sizing: border-box;
}

.radio-group {
    display: flex;
    gap: 32rpx;
}

.radio-item {
    font-size: 30rpx;
    color: #666;
    padding: 8rpx 24rpx;
    background: #f5f5f5;
    border-radius: 32rpx;

    &.active {
        background: $sull-color-primary;
        color: #fff;
    }
}

.submit-section {
    padding: 32rpx;
    background: #fff;
    position: fixed;
    bottom: 0;
    left: 0;
    width: 100%;
    box-sizing: border-box;
    box-shadow: 0 -4rpx 16rpx rgba(0, 0, 0, 0.05);
}

.submit-btn {
    background: $sull-color-primary;
    color: #fff;
    border-radius: 44rpx;
    font-size: 32rpx;
    font-weight: bold;
    height: 88rpx;
    line-height: 88rpx;

    &:active {
        opacity: 0.9;
    }

    &[disabled] {
        background: #ccc;
        color: #fff;
    }
}

/* Tabs styling integrated via CapsuleTabs component */

/* List & Modal Styles */
.list-header {
    display: flex;
    justify-content: flex-start;
    margin-bottom: 24rpx;
    padding-top: 20rpx;
}

.add-btn {
    background: $sull-color-primary;
    color: #fff;
    font-size: 28rpx;
    padding: 10rpx 30rpx;
    border-radius: 30rpx;
    display: flex;
    align-items: center;
    line-height: normal;
}

.plus {
    font-size: 32rpx;
    margin-right: 6rpx;
    margin-top: -4rpx;
}

.staff-list {
    min-height: 200rpx;
}

.staff-item {
    display: flex;
    justify-content: space-between;
    align-items: center;
    padding: 24rpx 0;
    border-bottom: 1rpx solid #EBEEF5;
}

.staff-info {
    display: flex;
    align-items: center;
}

.staff-name {
    font-size: 30rpx;
    color: #333;
    width: 160rpx;
    font-weight: bold;
}

.staff-role {
    font-size: 26rpx;
    color: #666;
    background: #F0F2F5;
    padding: 4rpx 16rpx;
    border-radius: 8rpx;
}

.delete-btn {
    color: #FF4D4F;
    font-size: 26rpx;
    padding: 10rpx;
}

/* Medicine List Styling */
.medicine-list {
    min-height: 200rpx;
}

.medicine-item {
    display: flex;
    justify-content: space-between;
    align-items: center;
    padding: 24rpx 0;
    border-bottom: 1rpx solid #EBEEF5;
}

.medicine-info {
    display: flex;
    flex-direction: column;
}

.medicine-name {
    font-size: 30rpx;
    color: #333;
    font-weight: bold;
}

.medicine-spec {
    font-size: 24rpx;
    color: #999;
    margin-top: 4rpx;
}

.medicine-qty {
    display: flex;
    align-items: baseline;
    gap: 4rpx;
}

.qty-num {
    font-size: 32rpx;
    color: $sull-color-primary;
    font-weight: bold;
}

.qty-unit {
    font-size: 24rpx;
    color: #666;
}

.empty-text {
    text-align: center;
    color: #999;
    padding: 40rpx;
    font-size: 26rpx;
}

/* Staff details styling */
.staff-list {
    min-height: 200rpx;
}

/* Custom Refresher */
.custom-refresher {
    height: 120rpx;
    width: 100%;
    display: flex;
    align-items: center;
    justify-content: center;
    position: absolute;
    top: -120rpx;
    left: 0;
    transition: transform 0.2s linear;
}

.refresher-content {
    display: flex;
    flex-direction: column;
    align-items: center;
    gap: 8rpx;
}

.refresher-icon {
    width: 64rpx;
    height: 64rpx;
    background-image: url("data:image/svg+xml;charset=utf-8,%3Csvg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 24 24' fill='%23009A97'%3E%3Cpath d='M19 14h-1V8h-3V6h-2v2h-2V6H9v2H7v6H6c-1.1 0-2 .9-2 2v3c0 1.1.9 2 2 2h1c0 1.66 1.34 3 3 3s3-1.34 3-3h4c0 1.66 1.34 3 3 3s3-1.34 3-3h1c1.1 0 2-.9 2-2v-3c0-1.1-.9-2-2-2zM8 19c-.55 0-1-.45-1-1s.45-1 1-1 1 .45 1 1-.45 1-1 1zm10 0c-.55 0-1-.45-1-1s.45-1 1-1 1 .45 1 1-.45 1-1 1zM8 8h2v2H8V8zm0 4h2v2H8v-2zm4 4h-2v-2h2v2zm0-4h-2V8h2v4zm4 4h-2v-2h2v2zm0-4h-2V8h2v4z'/%3E%3Cpath d='M4 8h2v2H4zm0 4h2v2H4z' fill-opacity='.3'/%3E%3C/svg%3E");
    background-size: contain;
    background-repeat: no-repeat;
    transition: transform 0.3s ease;
}

.refresher-icon.animating {
    animation: ambulance-bounce 0.8s infinite ease-in-out;
}

.refresher-text {
    font-size: 20rpx;
    color: #009A97;
    opacity: 0.8;
}

@keyframes ambulance-bounce {

    0%,
    100% {
        transform: translateY(0);
    }

    50% {
        transform: translateY(-6px);
    }
}
</style>
