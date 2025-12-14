<template>
    <view class="container">
        <!-- Header -->
        <view class="header">
            <BackButton class="header-back-btn" @click="handleBack" />
            <text class="header-title">{{ currentDept ? (isEditing ? $t('department.title_update') : deptTitle) :
                $t('department.title_info') }}</text>
        </view>

        <scroll-view scroll-y class="content" refresher-enabled="true" :refresher-triggered="isRefreshing"
            @refresherrefresh="onRefresh" refresher-default-style="none" refresher-background="#f0f5ff">
            <view slot="refresher" class="custom-refresher">
                <view class="refresher-content">
                    <view class="refresher-icon" :class="{ 'animating': isRefreshing }"></view>
                    <text class="refresher-text">{{ isRefreshing ? $t('index.refreshing') : $t('index.pullToRefresh')
                        }}</text>
                </view>
            </view>

            <!-- Loading State -->
            <view v-if="isLoading" class="loading-state">
                <text>Loading...</text>
            </view>

            <!-- Mode 1: Department Lists -->
            <view v-if="!currentDept && !isLoading" class="list-container">
                <!-- Org Existing Departments -->
                <view class="section-card">
                    <view class="search-box">
                        <view class="icon-search"></view>
                        <input class="search-input" type="text" v-model="searchKeyword"
                            :placeholder="$t('department.search_placeholder')" />
                    </view>
                    <view class="dept-grid-scroll">
                        <view class="dept-grid">
                            <view v-for="(dept, index) in filteredDeptList" :key="index" class="dept-grid-item"
                                :class="{ 'joined-item': isJoined(dept.Id) }">
                                <view class="dept-info-center">
                                    <text class="dept-name-small">{{ dept.Name }}</text>
                                    <view class="dept-duty-row-center">
                                        <view class="icon-user-small"></view>
                                        <text class="dept-sub-small">{{ dept.DutyUser || '-' }}</text>
                                    </view>
                                </view>
                                <view class="dept-action-center" v-if="!isJoined(dept.Id)">
                                    <button class="btn-join-small" @click="handleJoin(dept)">
                                        {{ $t('department.join') }}
                                    </button>
                                </view>
                                <view class="dept-action-center" v-else>
                                    <text class="joined-text">{{ $t('department.joined') }}</text>
                                </view>
                            </view>
                            <view v-if="filteredDeptList.length === 0" class="empty-state">
                                <text>{{ $t('department.no_dept_tip') }}</text>
                            </view>
                        </view>
                    </view>
                </view>

                <!-- User Joined Departments (Direct Display) -->
                <!-- User Joined Departments (Tabs: Info & Members) -->
                <view v-if="userDeptList.length > 0" class="my-dept-section">

                    <!-- Tabs -->
                    <view class="tabs">
                        <view class="tab-item" :class="{ active: currentTab === 0 }" @click="currentTab = 0">
                            <text>{{ $t('department.tab_info') }}</text>
                        </view>
                        <view class="tab-item" :class="{ active: currentTab === 1 }" @click="handleTabChange(1)">
                            <text>{{ $t('department.tab_members') }}{{ deptMembers.length > 0 ? ' (' +
                                deptMembers.length + ')' : '' }}</text>
                        </view>
                    </view>

                    <!-- Tab 1: Department Info -->
                    <view v-if="currentTab === 0" class="info-card">
                        <view class="info-row">
                            <text class="info-label">{{ $t('department.name') }}</text>
                            <text class="info-value">{{ userDeptList[0].Name }}</text>
                        </view>
                        <view class="info-row">
                            <text class="info-label">{{ $t('department.status') }}</text>
                            <view class="status-badge-small" :class="getStatusClass(userDeptList[0].AuditStatus)">
                                <text>{{ getStatusText(userDeptList[0].AuditStatus) }}</text>
                            </view>
                        </view>
                        <view class="info-row">
                            <text class="info-label">{{ $t('department.creator') }}</text>
                            <text class="info-value">{{ userDeptList[0].Creator }}</text>
                        </view>
                        <view class="info-row">
                            <text class="info-label">{{ $t('department.duty_user') }}</text>
                            <text class="info-value">{{ userDeptList[0].DutyUser || '-' }}</text>
                        </view>
                        <view class="info-row">
                            <text class="info-label">{{ $t('department.create_time') }}</text>
                            <text class="info-value">{{ userDeptList[0].CreationTime }}</text>
                        </view>
                        <view class="info-col" v-if="userDeptList[0].Remark" style="margin-top: 8px;">
                            <text class="info-label">{{ $t('department.remark') }}</text>
                            <text class="info-value remark-text">{{ userDeptList[0].Remark }}</text>
                        </view>
                        <view class="btn-container" style="margin-top: 16px; margin-bottom: 0;">
                            <button class="btn-primary" @click="openDetail(userDeptList[0], true)">{{
                                $t('department.edit') }}</button>
                            <button v-if="isJoined(userDeptList[0].Id)" class="btn-danger"
                                @click="handleQuit(userDeptList[0])">{{ $t('department.quit') }}</button>
                        </view>
                    </view>

                    <!-- Tab 2: Department Members -->
                    <view v-if="currentTab === 1" class="members-container">
                        <view v-for="(member, index) in deptMembers" :key="index" class="member-card">
                            <view class="member-info">
                                <view class="member-main">
                                    <text class="member-name">{{ member.UserName }}</text>
                                    <view v-if="isDutyUser(member)" class="duty-badge">
                                        <view class="icon-duty"></view>
                                        <text>{{ $t('department.duty_user') }}</text>
                                    </view>
                                </view>
                                <text class="member-time">{{ $t('department.member_join_time') }}: {{
                                    member.CreationTime || '-' }}</text>
                            </view>

                            <view class="member-action" v-if="!isDutyUser(member)">
                                <button class="btn-set-duty" @click="handleSetDuty(member)">
                                    {{ $t('department.set_duty') }}
                                </button>
                            </view>
                        </view>
                        <view v-if="deptMembers.length === 0" class="empty-state">
                            <text>{{ $t('handover.empty') }}</text>
                        </view>
                    </view>
                </view>
            </view>

            <!-- Mode 2: Detail View (Read/Edit) -->
            <view v-if="currentDept" class="form-container">

                <!-- View Mode: Read Only Display -->
                <view v-if="!isEditing" class="view-mode">
                    <view class="info-card">
                        <view class="info-row">
                            <text class="info-label">{{ $t('department.name') }}</text>
                            <text class="info-value">{{ currentDept.Name }}</text>
                        </view>
                        <view class="info-row">
                            <text class="info-label">{{ $t('department.status') }}</text>
                            <view class="status-badge-small" :class="getStatusClass(currentDept.AuditStatus)">
                                <text>{{ getStatusText(currentDept.AuditStatus) }}</text>
                            </view>
                        </view>
                        <view class="info-row">
                            <text class="info-label">{{ $t('department.creator') }}</text>
                            <text class="info-value">{{ currentDept.Creator }}</text>
                        </view>
                        <view class="info-row">
                            <text class="info-label">{{ $t('department.create_time') }}</text>
                            <text class="info-value">{{ currentDept.CreationTime }}</text>
                        </view>
                    </view>

                    <view class="info-card" v-if="currentDept.Remark">
                        <view class="info-col">
                            <text class="info-label">{{ $t('department.remark') }}</text>
                            <text class="info-value remark-text">{{ currentDept.Remark }}</text>
                        </view>
                    </view>

                    <view class="btn-container" v-if="isJoined(currentDept.Id)">
                        <button class="btn-danger" @click="handleQuit">{{ $t('department.quit') }}</button>
                    </view>
                </view>

                <!-- Edit Mode: Editable Fields -->
                <view v-else class="edit-mode">
                    <!-- Name -->
                    <view class="form-group">
                        <text class="label">{{ $t('department.name') }}</text>
                        <input class="input" type="text" v-model="currentDept.Name"
                            :placeholder="$t('department.name_placeholder')" />
                    </view>

                    <!-- Remark -->
                    <view class="form-group">
                        <text class="label">{{ $t('department.remark') }}</text>
                        <textarea class="textarea" v-model="currentDept.Remark"
                            :placeholder="$t('department.remark')"></textarea>
                    </view>
                </view>

                <!-- Action Button (Edit/Update) -->
                <view class="btn-container">
                    <button class="btn-primary" @click="handleEditAction">{{ isEditing ? $t('department.update') :
                        $t('department.edit') }}</button>
                </view>
            </view>
        </scroll-view>
    </view>
</template>

<script>
import request from '@/utils/request.js';
import BackButton from '@/components/BackButton/BackButton.vue';

export default {
    components: {
        BackButton
    },
    data() {
        return {
            isLoading: true,
            searchKeyword: '',
            isRefreshing: false,
            isEditing: false,
            currentTab: 0,
            deptMembers: [],

            // Lists
            orgDeptList: [],
            userDeptList: [],

            // Detail
            currentDept: null, // If null, show list. If object, show detail.
            userOrgInfo: null,
        }
    },
    computed: {
        deptTitle() {
            return this.currentDept ? this.currentDept.Name : this.$t('department.title_info');
        },
        filteredDeptList() {
            if (!this.searchKeyword) {
                return this.orgDeptList;
            }
            const keyword = this.searchKeyword.toLowerCase();
            return this.orgDeptList.filter(dept =>
                (dept.Name && dept.Name.toLowerCase().includes(keyword))
            );
        }
    },
    onLoad() {
        this.fetchAllData();
        this.userOrgInfo = uni.getStorageSync('userOrgInfo') || {};
    },
    methods: {
        handleBack() {
            if (this.currentDept) {
                this.currentDept = null;
                this.isEditing = false;
            } else {
                uni.navigateBack();
            }
        },
        onRefresh() {
            this.isRefreshing = true;
            const delay = new Promise(resolve => setTimeout(resolve, 1500));

            // Refresh all
            const fetch = this.fetchAllData(false); // pass false to not set main loading

            Promise.all([fetch, delay]).then(() => {
                this.isRefreshing = false;
                uni.showToast({
                    title: this.$t('index.refreshed'),
                    icon: 'none'
                });
            }).catch(() => {
                this.isRefreshing = false;
            });
        },
        fetchAllData(showLoading = true) {
            if (showLoading) this.isLoading = true;
            const t = this;

            const p1 = this.fetchOrgDepts();
            const p2 = this.fetchUserDepts();

            return Promise.all([p1, p2]).finally(() => {
                if (showLoading) t.isLoading = false;
            });
        },
        fetchOrgDepts() {
            // Assuming generic list endpoint or search
            // If API is strict, might needs modification
            return request.get('api/ssbh/OrganizationDepartment/OrganizationDepartment').then(res => {
                this.orgDeptList = res.rows || [];
                // this.orgDeptList = [].concat(this.orgDeptList).concat(this.orgDeptList).concat(this.orgDeptList).concat(this.orgDeptList);
            }).catch(err => {
                console.error("Fetch Org Depts Failed", err);
                this.orgDeptList = [];
            });
        },
        fetchUserDepts() {
            return request.get('api/ssbh/OrganizationDepartment/UserDepartment')
                .then(res => {
                    this.userDeptList = res.rows || [];
                    if (this.userDeptList.length > 0) {
                        this.fetchDeptMembers();
                    }
                })
                .catch(err => {
                    console.error("Failed to fetch user depts", err);
                    this.userDeptList = [];
                });
        },

        isJoined(deptId) {
            if (this.userOrgInfo.User) {
                return this.userOrgInfo.User.DepartmentId == deptId;
            }
            return false;
        },

        handleJoin(dept) {
            uni.showLoading({ title: 'Joining...' });
            const updateData = {
                ...this.userOrgInfo.User,
                DepartmentId: dept.Id,
            };
            request.post('api/ssbh/OrganizationUser/update', { "mainData": updateData, "detailData": null, "delKeys": null })
                .then(res => {
                    uni.hideLoading();
                    if (res.status) {
                        uni.showToast({ title: this.$t('department.success_join'), icon: 'success' });
                        this.userOrgInfo.User.DepartmentId = dept.Id;
                        uni.setStorageSync('userOrgInfo', this.userOrgInfo);
                        this.fetchAllData(false);
                    } else {
                        // Fallback: maybe specific API isn't 'join'.
                        // If 404, valid. For now, implement UI.
                        uni.showToast({ title: res.message || this.$t('department.fail_join'), icon: 'none' });
                    }
                })
                .catch(err => {
                    uni.hideLoading();
                    uni.showToast({ title: 'Error', icon: 'none' });
                });
        },

        openDetail(dept, edit = false) {
            this.currentDept = { ...dept }; // Clone to avoid direct mutate list
            this.isEditing = edit;
        },

        handleQuit(dept) {
            const target = dept || this.currentDept;
            const t = this;
            uni.showModal({
                title: this.$t('department.quit'),
                content: this.$t('department.quit_confirm'),
                success: function (res) {
                    if (res.confirm) {
                        t.executeQuit(target);
                    }
                }
            });
        },

        executeQuit(dept) {
            uni.showLoading();
            const updateData = {
                ...this.userOrgInfo.User,
                DepartmentId: null,
            };
            // Assuming 'quit' endpoint
            request.post('api/ssbh/OrganizationUser/update', { "mainData": updateData, "detailData": null, "delKeys": null })
                .then(res => {
                    uni.hideLoading();
                    if (res.status) {
                        uni.showToast({ title: this.$t('department.success_quit'), icon: 'success' });
                        this.currentDept = null;
                        this.userOrgInfo.User.DepartmentId = null;
                        uni.setStorageSync('userOrgInfo', this.userOrgInfo);
                        this.fetchAllData(false);
                    } else {
                        uni.showToast({ title: res.message || this.$t('department.fail_quit'), icon: 'none' });
                    }
                })
                .catch(e => {
                    uni.hideLoading();
                    uni.showToast({ title: 'Error', icon: 'none' });
                });
        },

        getStatusText(status) {
            const map = {
                0: this.$t('department.status_pending'),
                1: this.$t('department.status_approved'),
                2: this.$t('department.status_expired')
            };
            return map[status] || '-';
        },
        getStatusClass(status) {
            const map = {
                0: 'status-pending',
                1: 'status-approved',
                2: 'status-expired'
            };
            return map[status] || '';
        },
        handleEditAction() {
            if (!this.isEditing) {
                this.isEditing = true;
            } else {
                this.handleUpdate();
            }
        },
        handleUpdate() {
            if (!this.currentDept.Name) {
                uni.showToast({ title: 'Department Name is required', icon: 'none' });
                return;
            }

            const updateData = {
                ...this.currentDept
            };

            request.post('api/ssbh/OrganizationDepartment/update', { "mainData": updateData, "detailData": null, "delKeys": null })
                .then(res => {
                    if (res.status) {
                        uni.showToast({ title: this.$t('department.success_update'), icon: 'success' });
                        this.isEditing = false;
                        this.fetchAllData(false);
                    } else {
                        uni.showToast({ title: res.message || this.$t('department.fail_update'), icon: 'none' });
                    }
                })
                .catch(err => {
                    uni.showToast({ title: 'Update Error', icon: 'none' });
                });
        },
        handleTabChange(index) {
            this.currentTab = index;
            if (index === 1 && this.deptMembers.length === 0) {
                this.fetchDeptMembers();
            }
        },
        fetchDeptMembers() {
            if (!this.userDeptList.length) return;
            request.get('api/ssbh/OrganizationUser/DepartmentUser')
                .then(res => {
                    this.deptMembers = res.rows || [];
                })
                .catch(err => {
                    console.error("Fetch Members Failed", err);
                });
        },
        isDutyUser(member) {
            if (!this.userDeptList.length) return false;
            return this.userDeptList[0].DutyUserId == member.UserId;
        },
        handleSetDuty(member) {
            const updatedName = member.UserTrueName || member.UserName;
            const t = this;
            uni.showModal({
                title: this.$t('department.set_duty'),
                content: this.$t('department.set_duty_confirm', { name: updatedName }).replace('{name}', updatedName),
                success: function (res) {
                    if (res.confirm) {
                        t.executeSetDuty(member);
                    }
                }
            });
        },
        executeSetDuty(member) {
            if (!this.userDeptList.length) return;
            const currentDept = this.userDeptList[0];
            const updateData = {
                ...currentDept,
                DutyUserId: member.UserId,
                DutyUser: member.UserName
            };

            request.post('api/ssbh/OrganizationDepartment/update', { "mainData": updateData, "detailData": null, "delKeys": null })
                .then(res => {
                    if (res.status) {
                        uni.showToast({ title: this.$t('department.success_set_duty'), icon: 'success' });
                        // Update local data
                        this.userDeptList[0].DutyUserId = member.UserId;
                        this.userDeptList[0].DutyUser = member.UserTrueName || member.UserName;
                    } else {
                        uni.showToast({ title: res.message || this.$t('department.fail_set_duty'), icon: 'none' });
                    }
                })
                .catch(err => {
                    uni.showToast({ title: 'Update Error', icon: 'none' });
                });
        }
    }
}
</script>

<style lang="scss" scoped>
@import '@/sull.scss';

/* Styles copied from organization.vue */
.container {
    display: flex;
    flex-direction: column;
    height: 100vh;
    background-color: $sull-color-background;
}

.header {
    padding: 44px 24px 20px;
    display: flex;
    align-items: center;
    background: #fff;
    box-shadow: 0 2px 10px rgba(0, 0, 0, 0.02);
    position: relative;
}

.header-back-btn {
    position: absolute;
    left: 24px;
    z-index: 10;
}

.header-title {
    font-size: 18px;
    font-weight: bold;
    color: $sull-text-main;
    width: 100%;
    text-align: center;
}

.content {
    flex: 1;
    height: 0;
    padding: 0;
    box-sizing: border-box;
    width: 100%;
}

.form-container {
    display: flex;
    flex-direction: column;
    gap: 24px;
    padding: 24px;
}

.info-text {
    padding: 16px;
    background: #E6F7FF;
    border-radius: 8px;
    color: #1890FF;
    font-size: 14px;
    margin-bottom: 8px;
}

.form-group {
    display: flex;
    flex-direction: column;
    gap: 8px;
}

.label {
    font-size: 14px;
    color: $sull-text-secondary;
    font-weight: 500;
}

.input {
    height: 50px;
    background: #fff;
    border-radius: 12px;
    padding: 0 16px;
    font-size: 14px;
    color: $sull-text-main;
    width: 100%;
    box-sizing: border-box;
}

.input.readonly {
    background: #f5f5f5;
    color: #999;
}

.textarea {
    height: 100px;
    background: #fff;
    border-radius: 12px;
    padding: 16px;
    font-size: 14px;
    color: $sull-text-main;
    width: 100%;
    box-sizing: border-box;
}

.textarea.readonly {
    background: #f5f5f5;
    color: #999;
}

.status-badge {
    display: inline-flex;
    padding: 6px 16px;
    border-radius: 20px;
    font-size: 14px;
    font-weight: 500;
    align-self: flex-start;
}

.status-badge-small {
    display: inline-flex;
    padding: 2px 8px;
    border-radius: 4px;
    font-size: 12px;
}

.status-pending {
    background: #E6F7FF;
    color: #1890FF;
}

.status-approved {
    background: #F6FFED;
    color: #52C41A;
}

.status-expired {
    background: #FFF1F0;
    color: #F5222D;
}

.btn-container {
    margin-top: 24px;
    margin-bottom: 40px;
    display: flex;
    flex-direction: column;
    gap: 16px;
}

.btn-primary {
    width: 100%;
    height: 50px;
    background: $sull-color-primary;
    color: #fff;
    border-radius: 25px;
    font-size: 16px;
    font-weight: 600;
    display: flex;
    align-items: center;
    justify-content: center;
}

.btn-danger {
    width: 100%;
    height: 50px;
    background: #FFF1F0;
    color: #F5222D;
    border-radius: 25px;
    font-size: 16px;
    font-weight: 600;
    display: flex;
    align-items: center;
    justify-content: center;
    border: 1px solid #FFCCC7;
}

/* List Views */
.list-container {
    padding: 24px;
    display: flex;
    flex-direction: column;
    gap: 24px;
}

.section-header {
    margin-bottom: 8px;
}

.section-title {
    font-size: 18px;
    font-weight: bold;
    color: $sull-text-main;
}

.dept-list {
    display: flex;
    flex-direction: column;
    gap: 16px;
}

.dept-card {
    background: #fff;
    padding: 16px;
    border-radius: 12px;
    display: flex;
    justify-content: space-between;
    align-items: center;
    box-shadow: 0 2px 8px rgba(0, 0, 0, 0.02);
}

.clickable:active {
    background: #fafafa;
}

.dept-info {
    display: flex;
    flex-direction: column;
    gap: 4px;
}

.dept-name {
    font-size: 16px;
    font-weight: 600;
    color: $sull-text-main;
}

.dept-sub {
    font-size: 12px;
    color: $sull-text-secondary;
}

.btn-join {
    background: #E6F7FF;
    color: #1890FF;
    font-size: 12px;
    padding: 6px 16px;
    border-radius: 16px;
    border: none;
    line-height: normal;
}

.btn-join::after {
    border: none;
}

.btn-disabled {
    background: #f5f5f5;
    color: #999;
}

.icon-arrow-right {
    width: 16px;
    height: 16px;
    background-color: #C4C4C4;
    -webkit-mask-image: url("data:image/svg+xml;charset=utf-8,%3Csvg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 24 24' fill='black'%3E%3Cpath d='M8.59 16.59L13.17 12 8.59 7.41 10 6l6 6-6 6-1.41-1.41z'/%3E%3C/svg%3E");
    -webkit-mask-size: contain;
    mask-size: contain;
    -webkit-mask-repeat: no-repeat;
    mask-repeat: no-repeat;
    -webkit-mask-position: center;
    mask-position: center;
    mask-image: url("data:image/svg+xml;charset=utf-8,%3Csvg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 24 24' fill='black'%3E%3Cpath d='M8.59 16.59L13.17 12 8.59 7.41 10 6l6 6-6 6-1.41-1.41z'/%3E%3C/svg%3E");
}

.icon-user-small {
    width: 14px;
    height: 14px;
    background-color: #999;
    -webkit-mask-image: url("data:image/svg+xml;charset=utf-8,%3Csvg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 24 24' fill='black'%3E%3Cpath d='M12 12c2.21 0 4-1.79 4-4s-1.79-4-4-4-4 1.79-4 4 1.79 4 4 4zm0 2c-2.67 0-8 1.34-8 4v2h16v-2c0-2.66-5.33-4-8-4z'/%3E%3C/svg%3E");
    -webkit-mask-size: contain;
    mask-size: contain;
    -webkit-mask-repeat: no-repeat;
    mask-repeat: no-repeat;
    -webkit-mask-position: center;
    mask-position: center;
    mask-image: url("data:image/svg+xml;charset=utf-8,%3Csvg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 24 24' fill='black'%3E%3Cpath d='M12 12c2.21 0 4-1.79 4-4s-1.79-4-4-4-4 1.79-4 4 1.79 4 4 4zm0 2c-2.67 0-8 1.34-8 4v2h16v-2c0-2.66-5.33-4-8-4z'/%3E%3C/svg%3E");
}

.dept-duty-row {
    display: flex;
    align-items: center;
    gap: 4px;
    margin-top: 4px;
}

.empty-state {
    padding: 20px;
    text-align: center;
    color: $sull-text-secondary;
    font-size: 14px;
    background: #fff;
    border-radius: 12px;
}

.loading-state {
    display: flex;
    justify-content: center;
    padding-top: 40px;
    color: $sull-text-secondary;
}

/* Custom Refresher */
.custom-refresher {
    height: 60px;
    width: 100%;
    display: flex;
    align-items: center;
    justify-content: center;
}

.refresher-content {
    display: flex;
    flex-direction: column;
    align-items: center;
    gap: 4px;
}

.refresher-icon {
    width: 32px;
    height: 32px;
    background-image: url("data:image/svg+xml;charset=utf-8,%3Csvg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 24 24' fill='%23409EFF'%3E%3Cpath d='M19 14h-1V8h-3V6h-2v2h-2V6H9v2H7v6H6c-1.1 0-2 .9-2 2v3c0 1.1.9 2 2 2h1c0 1.66 1.34 3 3 3s3-1.34 3-3h4c0 1.66 1.34 3 3 3s3-1.34 3-3h1c1.1 0 2-.9 2-2v-3c0-1.1-.9-2-2-2zM8 19c-.55 0-1-.45-1-1s.45-1 1-1 1 .45 1 1-.45 1-1 1zm10 0c-.55 0-1-.45-1-1s.45-1 1-1 1 .45 1 1-.45 1-1 1zM8 8h2v2H8V8zm0 4h2v2H8v-2zm4 4h-2v-2h2v2zm0-4h-2V8h2v4zm4 4h-2v-2h2v2zm0-4h-2V8h2v4z'/%3E%3Cpath d='M4 8h2v2H4zm0 4h2v2H4z' fill-opacity='.3'/%3E%3C/svg%3E");
    background-size: contain;
    background-repeat: no-repeat;
    transition: transform 0.3s ease;
}

.refresher-icon.animating {
    animation: ambulance-bounce 0.8s infinite ease-in-out;
}

.refresher-text {
    font-size: 10px;
    color: $sull-color-primary;
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

/* View Mode Styles */
.view-mode {
    display: flex;
    flex-direction: column;
    gap: 16px;
}

.info-card {
    background: #fff;
    border-radius: 16px;
    padding: 20px;
    display: flex;
    flex-direction: column;
    gap: 16px;
}

.info-row {
    display: flex;
    justify-content: space-between;
    align-items: center;
}

.info-col {
    display: flex;
    flex-direction: column;
    gap: 8px;
}

.info-label {
    font-size: 14px;
    color: $sull-text-secondary;
}

.info-value {
    font-size: 14px;
    color: $sull-text-main;
    font-weight: 500;
    text-align: right;
}

.remark-text {
    text-align: left;
    line-height: 1.5;
}

.edit-mode {
    display: flex;
    flex-direction: column;
    gap: 24px;
}

/* Grid Styles */
.section-card {
    background: #fff;
    border-radius: 16px;
    padding: 20px;
}

.dept-grid {
    display: flex;
    flex-wrap: wrap;
    gap: 12px;
}

.dept-grid-item {
    /* 3 items per row minus gaps: (100% - (2 * 12px)) / 3 */
    width: calc((100% - 24px) / 3);
    background: #F5F7FA;
    border-radius: 12px;
    padding: 12px 8px;
    display: flex;
    flex-direction: column;
    align-items: center;
    gap: 8px;
    box-sizing: border-box;
}

.dept-info-center {
    display: flex;
    flex-direction: column;
    align-items: center;
    gap: 4px;
    width: 100%;
}

.dept-name-small {
    font-size: 14px;
    font-weight: 600;
    color: $sull-text-main;
    text-align: center;
    /* Handle long names */
    white-space: nowrap;
    overflow: hidden;
    text-overflow: ellipsis;
    width: 100%;
}

.search-box {
    background: #F5F7FA;
    border-radius: 8px;
    padding: 8px 12px;
    display: flex;
    align-items: center;
    gap: 8px;
    margin-bottom: 16px;
}

.icon-search {
    width: 16px;
    height: 16px;
    background-color: #999;
    -webkit-mask-image: url("data:image/svg+xml;charset=utf-8,%3Csvg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 24 24' fill='black'%3E%3Cpath d='M15.5 14h-.79l-.28-.27C15.41 12.59 16 11.11 16 9.5 16 5.91 13.09 3 9.5 3S3 5.91 3 9.5 5.91 16 9.5 16c1.61 0 3.09-.59 4.23-1.57l.27.28v.79l5 4.99L20.49 19l-4.99-5zm-6 0C7.01 14 5 11.99 5 9.5S7.01 5 9.5 5 14 7.01 14 9.5 14z'/%3E%3C/svg%3E");
    -webkit-mask-size: contain;
    mask-size: contain;
    -webkit-mask-repeat: no-repeat;
    mask-repeat: no-repeat;
    -webkit-mask-position: center;
    mask-position: center;
    mask-image: url("data:image/svg+xml;charset=utf-8,%3Csvg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 24 24' fill='black'%3E%3Cpath d='M15.5 14h-.79l-.28-.27C15.41 12.59 16 11.11 16 9.5 16 5.91 13.09 3 9.5 3S3 5.91 3 9.5 5.91 16 9.5 16c1.61 0 3.09-.59 4.23-1.57l.27.28v.79l5 4.99L20.49 19l-4.99-5zm-6 0C7.01 14 5 11.99 5 9.5S7.01 5 9.5 5 14 7.01 14 9.5 14z'/%3E%3C/svg%3E");
}

.search-input {
    font-size: 14px;
    color: $sull-text-main;
    flex: 1;
    background: transparent;
    border: none;
    outline: none;
    height: 24px;
    line-height: 24px;
}

.dept-grid-scroll {
    height: 202px;
    overflow-y: auto;
}

.dept-duty-row-center {
    display: flex;
    align-items: center;
    justify-content: center;
    gap: 4px;
    width: 100%;
}

.dept-sub-small {
    font-size: 11px;
    color: $sull-text-secondary;
}

.dept-action-center {
    width: 100%;
    display: flex;
    justify-content: center;
}

.btn-join-small {
    background: #E6F7FF;
    color: #1890FF;
    font-size: 11px;
    padding: 0;
    height: 24px;
    line-height: 24px;
    width: 80%;
    border-radius: 12px;
    border: none;
    text-align: center;
}

.btn-join-small.btn-disabled {
    background: #e0e0e0;
    color: #fff;
    /* or gray */
}

.btn-join-small::after {
    border: none;
}

.joined-item {
    background: #E6F7FF;
    /* Light blue background for joined items */
    border: 1px solid #91D5FF;
}

.joined-text {
    font-size: 11px;
    color: $sull-text-secondary;
    font-weight: 500;
}

/* Override empty state if needed or reuse */

/* Tabs */
.tabs {
    display: flex;
    background: #fff;
    border-radius: 25px;
    padding: 4px;
    margin-bottom: 20px;
    box-shadow: 0 2px 8px rgba(0, 0, 0, 0.02);
}

.tab-item {
    flex: 1;
    height: 42px;
    display: flex;
    align-items: center;
    justify-content: center;
    font-size: 14px;
    color: $sull-text-secondary;
    font-weight: 500;
    border-radius: 21px;
    transition: all 0.3s;
}

.tab-item.active {
    background: $sull-color-primary;
    /* Using a lighter blue similar to image or fallback to primary if variable not perfect match */
    color: #fff;
    font-weight: 600;
    box-shadow: 0 2px 6px rgba(149, 202, 255, 0.4);
}

/* Removed tab-indicator styles */

/* Members List */
.members-container {
    display: flex;
    flex-direction: column;
    gap: 12px;
}

.member-card {
    background: #fff;
    border-radius: 12px;
    padding: 16px;
    display: flex;
    justify-content: space-between;
    align-items: center;
    box-shadow: 0 1px 4px rgba(0, 0, 0, 0.02);
}

.member-info {
    display: flex;
    flex-direction: column;
    gap: 6px;
}

.member-main {
    display: flex;
    align-items: center;
    gap: 8px;
}

.member-name {
    font-size: 15px;
    font-weight: 600;
    color: $sull-text-main;
}

.duty-badge {
    display: flex;
    align-items: center;
    gap: 4px;
    background: #FFF7E6;
    color: #FA8C16;
    padding: 2px 8px;
    border-radius: 4px;
    font-size: 11px;
}

.icon-duty {
    width: 12px;
    height: 12px;
    background-color: #FA8C16;
    -webkit-mask-image: url("data:image/svg+xml;charset=utf-8,%3Csvg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 24 24' fill='black'%3E%3Cpath d='M12 1L3 5v6c0 5.55 3.84 10.74 9 12 5.16-1.26 9-6.45 9-12V5l-9-4zm0 10.99h7c-.53 4.12-3.28 7.79-7 8.94V12H5V6.3l7-3.11v8.8z'/%3E%3C/svg%3E");
    -webkit-mask-size: contain;
    mask-size: contain;
    -webkit-mask-repeat: no-repeat;
    mask-repeat: no-repeat;
    -webkit-mask-position: center;
    mask-position: center;
    mask-image: url("data:image/svg+xml;charset=utf-8,%3Csvg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 24 24' fill='black'%3E%3Cpath d='M12 1L3 5v6c0 5.55 3.84 10.74 9 12 5.16-1.26 9-6.45 9-12V5l-9-4zm0 10.99h7c-.53 4.12-3.28 7.79-7 8.94V12H5V6.3l7-3.11v8.8z'/%3E%3C/svg%3E");
}

.member-time {
    font-size: 12px;
    color: $sull-text-secondary;
}

.btn-set-duty {
    background: #E6F7FF;
    color: #1890FF;
    font-size: 11px;
    padding: 0 12px;
    height: 24px;
    line-height: 24px;
    border-radius: 12px;
    border: none;
}

.btn-set-duty:active {
    background: #D6EFFF;
}

.btn-set-duty::after {
    border: none;
}
</style>
