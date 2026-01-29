<template>
    <view class="container">
        <view class="header" :style="{ paddingTop: statusBarHeight * 2 + 'rpx' }">
            <view class="header-content">
                <BackButton class="header-back-btn" @click="handleBack" />
                <text class="header-title">{{ currentDept ? (isEditing ? (isCreating ? $t('department.title_create') :
                    $t('department.title_update')) : deptTitle) :
                    $t('department.title_info') }}</text>
            </view>
        </view>

        <scroll-view scroll-y class="content" refresher-enabled="true" :refresher-triggered="isRefreshing"
            @refresherrefresh="onRefresh" @refresherpulling="onPulling" @refresherrestore="onRestore"
            refresher-default-style="none" refresher-background="#f0f5ff">
            <CustomRefresher slot="refresher" :isRefreshing="isRefreshing" :isPulling="isPulling" />

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
                                :class="{ 'joined-item': isJoined(dept.Id), 'selected-item': selectedDeptId === dept.Id }"
                                @click="handleDeptSelect(dept)">
                                <view class="dept-info-center">
                                    <text class="dept-name-small">{{ dept.Name }}</text>
                                    <view class="dept-duty-row-center">
                                        <view class="icon-user-small"></view>
                                        <text class="dept-sub-small">{{ dept.DutyUser || '-' }}</text>
                                    </view>
                                </view>
                                <view class="dept-action-center" v-if="!isJoined(dept.Id)">
                                    <button class="btn-join-small" @click.stop="handleJoin(dept)">
                                        {{ $t('department.join') }}
                                    </button>
                                </view>
                                <view class="dept-action-center" v-else>
                                    <view class="joined-badge">
                                        <text class="joined-text">{{ $t('department.joined') }}</text>
                                    </view>
                                </view>
                            </view>
                            <view v-if="filteredDeptList.length === 0" class="empty-state">
                                <text>{{ $t('department.no_dept_tip') }}</text>
                                <button class="btn-primary" style="margin-top: 32rpx; width: 240rpx;"
                                    @click="handlePrepareCreate">
                                    {{ $t('department.create') }}
                                </button>
                            </view>
                        </view>
                    </view>
                </view>

                <!-- User Joined Departments (Direct Display) -->
                <!-- User Joined Departments (Tabs: Info & Members) -->
                <view v-if="selectedDeptId" class="my-dept-section">

                    <!-- Tabs -->
                    <view class="tabs">
                        <view class="tab-item" :class="{ active: currentTab === 0 }" @click="currentTab = 0">
                            <text>{{ $t('department.tab_info') }}</text>
                        </view>
                        <view class="tab-item" :class="{ active: currentTab === 1 }" @click="handleTabChange(1)">
                            <text>{{ $t('department.tab_members') }}</text>
                        </view>
                    </view>

                    <!-- Tab 1: Department Info -->
                    <view v-if="currentTab === 0" class="info-card">
                        <view class="info-row">
                            <text class="info-label">{{ $t('department.name') }}</text>
                            <view class="value-with-action">
                                <text class="info-value">{{ selectedDept?.Name }}</text>
                                <view class="dots-btn" @click="openDeptMenu(selectedDept)">
                                    <view class="dot"></view>
                                    <view class="dot"></view>
                                    <view class="dot"></view>
                                </view>
                            </view>
                        </view>
                        <view class="info-row">
                            <text class="info-label">{{ $t('department.status') }}</text>
                            <view class="status-badge-small" :class="getStatusClass(selectedDept?.AuditStatus)">
                                <text>{{ getStatusText(selectedDept?.AuditStatus) }}</text>
                            </view>
                        </view>
                        <view class="info-row">
                            <text class="info-label">{{ $t('department.creator') }}</text>
                            <text class="info-value">{{ selectedDept?.Creator }}</text>
                        </view>
                        <view class="info-row">
                            <text class="info-label">{{ $t('department.duty_user') }}</text>
                            <text class="info-value">{{ selectedDept?.DutyUser || '-' }}</text>
                        </view>
                        <view class="info-row">
                            <text class="info-label">{{ $t('department.create_time') }}</text>
                            <text class="info-value">{{ selectedDept?.CreationTime }}</text>
                        </view>
                        <view class="info-col" v-if="selectedDept?.Remark" style="margin-top: 16rpx;">
                            <text class="info-label">{{ $t('department.remark') }}</text>
                            <text class="info-value remark-text">{{ selectedDept?.Remark }}</text>
                        </view>
                        <view class="btn-container" v-if="isCreator(selectedDept)"
                            style="margin-top: 32rpx; margin-bottom: 0;">
                            <button class="btn-primary" @click="openDetail(selectedDept, true)">{{
                                $t('department.edit') }}</button>
                        </view>
                    </view>

                    <!-- Tab 2: Department Members -->
                    <view v-if="currentTab === 1" class="members-container">
                        <view v-for="(member, index) in deptMembers" :key="index" class="member-card">
                            <view class="member-info">
                                <view class="member-main">
                                    <text class="member-name">{{ member.UserName }}</text>
                                    <text v-if="member.Title" class="member-title">{{ member.Title }}</text>
                                    <view v-if="isDutyUser(member)" class="duty-badge">
                                        <view class="icon-duty"></view>
                                        <text>{{ $t('department.duty_user') }}</text>
                                    </view>
                                </view>
                                <text class="member-time">{{ $t('department.member_join_time') }}: {{
                                    member.CreationTime || '-' }}</text>
                            </view>

                            <view class="member-action">
                                <view class="dots-btn" @click="openMemberMenu(member)">
                                    <view class="dot"></view>
                                    <view class="dot"></view>
                                    <view class="dot"></view>
                                </view>
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
                    <button class="btn-primary" @click="handleEditAction">{{ isEditing ? (isCreating ?
                        $t('department.create') : $t('department.update')) :
                        $t('department.edit') }}</button>
                </view>
            </view>
        </scroll-view>

        <!-- Action Sheet -->
        <view class="action-sheet-mask" v-if="showActionSheet" @click="showActionSheet = false">
            <view class="action-sheet" @click.stop>
                <view class="action-header">
                    <text class="action-title">{{ menuType === 'member' ? (selectedMember?.UserTrueName ||
                        selectedMember?.UserName) : selectedDept?.Name }}</text>
                </view>
                <view class="action-list">
                    <!-- Member Actions -->
                    <template v-if="menuType === 'member'">
                        <view class="action-item" v-if="currentUserLevel >= 40 && !isDutyUser(selectedMember)"
                            @click="handleActionSelection('setDuty')">
                            <text>设置责任人</text>
                        </view>
                        <view class="action-item" v-if="currentUserLevel >= 30"
                            @click="handleActionSelection('setPosition')">
                            <text>设置职别</text>
                        </view>
                    </template>
                    <!-- Department Actions -->
                    <template v-else-if="menuType === 'dept'">
                        <view class="action-item" @click="handleActionSelection('switch')">
                            <text>切换到此部门</text>
                        </view>
                        <view class="action-item" style="color: #FF4D4F;" v-if="isJoined(selectedDept.Id)"
                            @click="handleActionSelection('quit')">
                            <text>{{ $t('department.quit') }}</text>
                        </view>
                    </template>
                </view>
                <view class="action-cancel" @click="showActionSheet = false">
                    <text>{{ $t('common.cancel') }}</text>
                </view>
            </view>
        </view>

        <!-- Position Selection Modal -->
        <view class="modal-mask" v-if="showPositionModal" @click="showPositionModal = false"
            @touchmove.stop.prevent="doNothing">
            <view class="modal-content position-modal" @click.stop>
                <view class="modal-header">
                    <text class="modal-title">{{ $t('department.set_position') || '设置职别' }}</text>
                    <view class="close-btn" @click="showPositionModal = false">✕</view>
                </view>
                <view class="modal-body">
                    <view class="position-group" v-for="group in sortedGroups" :key="group">
                        <view class="level-row">
                            <view class="position-tag" v-for="(pos, idx) in getPositionsByGroup(group)" :key="idx"
                                :class="{ active: isPositionSelected(pos.title) }" @click="togglePosition(pos)">
                                {{ pos.title }}
                            </view>
                        </view>
                    </view>
                </view>
                <view class="modal-footer">
                    <button class="btn-primary" @click="submitPositionChange">{{ $t('common.confirm') }}</button>
                </view>
            </view>
        </view>
    </view>
</template>

<script>
import request from '@/utils/request.js';
import { storage } from '@/utils/storage.js';
import BackButton from '@/components/BackButton/BackButton.vue';
import FooterInfo from '@/components/FooterInfo/FooterInfo.vue';
import WarningModal from '@/components/WarningModal/WarningModal.vue';
import CustomRefresher from '@/components/CustomRefresher/CustomRefresher.vue';
import { formatMessage } from '@/utils/i18n.js';

export default {
    components: {
        BackButton,
        FooterInfo,
        WarningModal,
        CustomRefresher
    },
    data() {
        return {
            isLoading: true,
            searchKeyword: '',
            isRefreshing: false,
            isPulling: false,
            statusBarHeight: 20,
            isEditing: false,
            isCreating: false,
            currentTab: 0,
            deptMembers: [],

            // Lists
            orgDeptList: [],
            userDeptList: [],

            // Detail
            currentDept: null, // If null, show list. If object, show detail.
            userOrgInfo: null,

            // New Menu & Position Logic
            showActionSheet: false,
            menuType: 'member', // 'member' or 'dept'
            showPositionModal: false,
            selectedMember: null,
            selectedDeptId: null,
            selectedPositions: [],
            positionData: [
                {
                    "title": "院长", "level": 60, "group": 60, menus: ['index.actions.handover', 'index.actions.stockIn', 'index.actions.itemOutbound',
                        'index.actions.virtualPos', 'index.actions.sealing', 'index.actions.records', 'index.actions.cleaning', 'index.actions.itemManage'
                    ]
                },
                {
                    "title": "书记", "level": 61, "group": 60, menus: ['index.actions.handover', 'index.actions.stockIn', 'index.actions.itemOutbound',
                        'index.actions.virtualPos', 'index.actions.sealing', 'index.actions.records', 'index.actions.cleaning', 'index.actions.itemManage'
                    ]
                },
                {
                    "title": "分管院长", "level": 50, "group": 50, menus: ['index.actions.handover', 'index.actions.stockIn', 'index.actions.itemOutbound',
                        'index.actions.virtualPos', 'index.actions.sealing', 'index.actions.records', 'index.actions.cleaning', 'index.actions.itemManage'
                    ]
                },
                {
                    "title": "护理部主任", "level": 40, "group": 40, menus: ['index.actions.handover', 'index.actions.stockIn', 'index.actions.itemOutbound',
                        'index.actions.virtualPos', 'index.actions.sealing', 'index.actions.records', 'index.actions.cleaning', 'index.actions.itemManage']
                },
                {
                    "title": "科主任", "level": 31, "group": 30, menus: ['index.actions.handover', 'index.actions.stockIn', 'index.actions.itemOutbound',
                        'index.actions.virtualPos', 'index.actions.sealing', 'index.actions.records', 'index.actions.cleaning', 'index.actions.itemManage']
                },
                {
                    "title": "护士长", "level": 30, "group": 30, menus: ['index.actions.handover', 'index.actions.stockIn', 'index.actions.itemOutbound',
                        'index.actions.virtualPos', 'index.actions.sealing', 'index.actions.records', 'index.actions.cleaning', 'index.actions.itemManage']
                },
                { "title": "医生", "level": 22, "group": 20, menus: ['index.actions.virtualPos', 'index.actions.records'] },
                { "title": "护士", "level": 20, "group": 20, menus: ['index.actions.handover', 'index.actions.stockIn', 'index.actions.itemOutbound', 'index.actions.virtualPos', 'index.actions.records'] },
                { "title": "技师", "level": 21, "group": 20, menus: ['index.actions.handover', 'index.actions.stockIn', 'index.actions.itemOutbound', 'index.actions.virtualPos', 'index.actions.records'] },
                { "title": "护工", "level": 11, "group": 10, menus: ['index.actions.cleaning'] },
                { "title": "后勤", "level": 10, "group": 10, menus: ['index.actions.cleaning'] }
            ]
        }
    },
    computed: {
        isSuperAdmin() {
            const userInfo = storage.get('userInfo');
            return userInfo?.roleName === '超级管理员';
        },
        currentUserLevel() {
            if (this.isSuperAdmin) return 999;
            const userOrgInfo = storage.get('userOrgInfo');
            return userOrgInfo?.User?.Level || 0;
        },
        sortedGroups() {
            const visiblePositions = this.positionData.filter(p => p.level < this.currentUserLevel || this.isSuperAdmin);
            const groups = [...new Set(visiblePositions.map(p => p.group))];
            return groups.sort((a, b) => b - a);
        },
        selectedDept() {
            if (!this.selectedDeptId) return null;
            return this.orgDeptList.find(d => d.Id === this.selectedDeptId);
        },
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
        },
        canSetDuty() {
            const userOrgInfo = storage.get('userOrgInfo');
            if (!this.selectedDept || !userOrgInfo || !userOrgInfo.User) return false;
            const dept = this.selectedDept;
            const userId = userOrgInfo.User.UserId;

            const hasDutyUser = dept.DutyUserId && dept.DutyUser;

            // Condition 1: No duty user, and current user is Creator
            if (!hasDutyUser) {
                return dept.CreatorId == userId;
            }

            // Condition 2: Has duty user, and current user IS the duty user
            if (hasDutyUser) {
                return dept.DutyUserId == userId;
            }

            return false;
        }
    },
    onLoad() {
        const sysInfo = uni.getSystemInfoSync();
        this.statusBarHeight = sysInfo.statusBarHeight || 20;
        this.fetchAllData();
        this.userOrgInfo = storage.get('userOrgInfo') || {};
    },
    onBackPress(options) {
        // 1. Close Modals
        if (this.showActionSheet || this.showPositionModal) {
            this.showActionSheet = false;
            this.showPositionModal = false;
            return true;
        }
        // 2. Close Detail View
        if (this.currentDept) {
            this.currentDept = null;
            this.isEditing = false;
            this.isCreating = false;
            return true;
        }
        // 3. Check Stack
        const pages = getCurrentPages();
        if (pages.length <= 1) {
            // Navigate to Profile/Home if no history
            uni.reLaunch({
                url: '/pages/profile/profile'
            });
            return true;
        }
        // Default: Allow navigateBack
        return false;
    },
    methods: {
        handleBack() {
            if (this.currentDept) {
                this.currentDept = null;
                this.isEditing = false;
                this.isCreating = false;
            } else {
                uni.navigateBack();
            }
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

            // Refresh all
            const fetch = this.fetchAllData(false); // pass false to not set main loading

            fetch.then(() => {
                setTimeout(() => {
                    this.isRefreshing = false;
                    this.isPulling = false;
                    uni.showToast({
                        title: this.$t('index.refreshed'),
                        icon: 'none'
                    });
                }, 800);
            }).catch(() => {
                this.isRefreshing = false;
                this.isPulling = false;
            });
        },
        fetchAllData(showLoading = true) {
            if (showLoading) this.isLoading = true;
            const t = this;

            const p1 = this.fetchOrgDepts();
            const p2 = this.fetchUserDepts();
            const p3 = this.refreshUserOrgInfo();

            return Promise.all([p1, p2, p3]).then(() => {
                // Default select own department - Removed as per request to hide info by default
                /*
                if (!this.selectedDeptId && this.userOrgInfo?.User?.DepartmentId) {
                    this.selectedDeptId = this.userOrgInfo.User.DepartmentId;
                }
                */
            }).finally(() => {
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
                    if (res.status || res.success) {
                        uni.showToast({ title: this.$t('department.success_join'), icon: 'success' });
                        this.refreshUserOrgInfo().then(() => {
                            this.selectedDeptId = dept.Id;
                            this.fetchAllData(false);
                        });
                    } else {
                        uni.showToast({ title: res.message || this.$t('department.fail_join'), icon: 'none' });
                    }
                })
                .catch(err => {
                    uni.hideLoading();
                    uni.showToast({ title: 'Error', icon: 'none' });
                });
        },
        handleDeptSelect(dept) {
            if (this.selectedDeptId === dept.Id) {
                this.selectedDeptId = null;
            } else {
                this.selectedDeptId = dept.Id;
                this.currentTab = 0; // Reset to info tab
                this.deptMembers = []; // Clear members to force refresh 
            }
        },
        isCreator(dept) {
            if (!dept || !this.userOrgInfo?.User) return false;
            return dept.CreatorId == this.userOrgInfo.User.UserId;
        },

        openDetail(dept, edit = false) {
            this.currentDept = { ...dept }; // Clone to avoid direct mutate list
            this.isEditing = edit;
            this.isCreating = false;
        },
        handlePrepareCreate() {
            this.currentDept = {
                Name: '',
                Remark: '',
                OrganizationId: (this.userOrgInfo?.Organization?.Id) || (this.userOrgInfo?.OrganizationId)
            };
            this.isEditing = true;
            this.isCreating = true;
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
                        this.refreshUserOrgInfo().then(() => {
                            this.fetchAllData(false);
                        });
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

            const url = this.isCreating ? 'api/ssbh/OrganizationDepartment/Add' : 'api/ssbh/OrganizationDepartment/update';

            request.post(url, { "mainData": updateData, "detailData": null, "delKeys": null })
                .then(res => {
                    if (res.status || res.success) {
                        uni.showToast({ title: this.isCreating ? this.$t('department.success_create') : this.$t('department.success_update'), icon: 'success' });
                        this.isEditing = false;
                        this.isCreating = false;
                        this.currentDept = null;
                        this.fetchAllData(false);
                    } else {
                        uni.showToast({ title: res.message || (this.isCreating ? this.$t('department.fail_create') : this.$t('department.fail_update')), icon: 'none' });
                    }
                })
                .catch(err => {
                    uni.showToast({ title: this.isCreating ? 'Create Error' : 'Update Error', icon: 'none' });
                });
        },
        refreshUserOrgInfo() {
            return request.get('api/ssbh/OrganizationUser/UserOrganization').then(res => {
                if (res) {
                    storage.set('userOrgInfo', res);
                    this.userOrgInfo = res;
                }
                return res;
            }).catch(err => {
                console.error("Refresh UserOrgInfo Failed", err);
            });
        },
        handleTabChange(index) {
            this.currentTab = index;
            if (index === 1 && this.deptMembers.length === 0) {
                this.fetchDeptMembers();
            }
        },
        fetchDeptMembers() {
            if (!this.selectedDeptId) return;
            // Get members for the SELECTED department
            const form = {
                page: 1,
                rows: 1000,
                sort: 'CreationTime',
                order: 'desc',
                wheres: JSON.stringify([
                    { name: 'DepartmentId', value: this.selectedDeptId }
                ])
            };
            request.post('api/ssbh/OrganizationUser/GetPageData', form)
                .then(res => {
                    this.deptMembers = res.rows || [];
                })
                .catch(err => {
                    console.error("Fetch Members Failed", err);
                });
        },
        isDutyUser(member) {
            if (!this.selectedDept) return false;
            return this.selectedDept.DutyUserId == member.UserId;
        },
        handleSetDuty(member) {
            const updatedName = member.UserTrueName || member.UserName;
            const t = this;
            uni.showModal({
                title: this.$t('department.set_duty'),
                // #ifdef H5
                content: this.$t('department.set_duty_confirm', { name: updatedName }),
                // #endif
                // #ifndef H5
                content: formatMessage(this.$t('department.set_duty_confirm'), { name: updatedName }),
                // #endif
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
                    if (res.status || res.success) {
                        uni.showToast({ title: this.$t('department.success_set_duty'), icon: 'success' });
                        // Update local data
                        if (this.selectedDept) {
                            this.selectedDept.DutyUserId = member.UserId;
                            this.selectedDept.DutyUser = member.UserTrueName || member.UserName;
                        }
                    } else {
                        uni.showToast({ title: res.message || this.$t('department.fail_set_duty'), icon: 'none' });
                    }
                })
                .catch(err => {
                    uni.showToast({ title: this.$t('common.update_error'), icon: 'none' });
                });
        },
        doNothing() { },

        // Menu & Position Methods
        openMemberMenu(member) {
            this.selectedMember = member;
            this.menuType = 'member';
            this.showActionSheet = true;
        },
        openDeptMenu(dept) {
            this.menuType = 'dept';
            this.showActionSheet = true;
        },
        handleActionSelection(action) {
            this.showActionSheet = false;
            if (action === 'setDuty') {
                this.handleSetDuty(this.selectedMember);
            } else if (action === 'setPosition') {
                // Initialize selection from member title if possible
                this.selectedPositions = this.selectedMember.Title ? this.selectedMember.Title.split(',') : [];
                this.showPositionModal = true;
            } else if (action === 'switch') {
                this.handleSwitchDept();
            } else if (action === 'quit') {
                this.handleQuit(this.selectedDept);
            }
        },
        handleSwitchDept() {
            if (!this.selectedDeptId) return;
            uni.showLoading({ title: 'Switching...' });

            // Call SetUserInfo just like organization switching
            request.post('api/ssbh/OrganizationDepartment/SetUserInfo', {
                mainData: {
                    OrganizationId: this.selectedDept?.OrganizationId,
                    DepartmentId: this.selectedDeptId
                }
            })
                .then(res => {
                    uni.hideLoading();
                    if (res.status || res.success) {
                        uni.showToast({ title: '切换成功', icon: 'success' });
                        // Notify other pages
                        uni.$emit('refreshUserOrgInfo');
                        this.refreshUserOrgInfo();
                    } else {
                        uni.showToast({ title: res.message || '切换失败', icon: 'none' });
                    }
                })
                .catch(err => {
                    uni.hideLoading();
                    uni.showToast({ title: 'Error', icon: 'none' });
                });
        },
        getPositionsByGroup(group) {
            return this.positionData.filter(p => p.group === group && (p.level < this.currentUserLevel || this.isSuperAdmin));
        },
        isPositionSelected(title) {
            return this.selectedPositions.includes(title);
        },
        togglePosition(pos) {
            // Single selection logic: clicking the same one toggles it, clicking another switches to it
            if (this.selectedPositions.includes(pos.title)) {
                this.selectedPositions = [];
            } else {
                this.selectedPositions = [pos.title];
            }
        },
        submitPositionChange() {
            if (!this.selectedMember) return;

            // Calculate highest level from selected positions
            let highestLevel = 0;
            if (this.selectedPositions.length > 0) {
                const levels = this.selectedPositions.map(title => {
                    const p = this.positionData.find(pd => pd.title === title);
                    return p ? p.level : 0;
                });
                highestLevel = Math.max(...levels);
            }

            const updateData = {
                ...this.selectedMember,
                Title: this.selectedPositions.join(','),
                Level: highestLevel
            };

            uni.showLoading({ title: 'Updating...' });
            request.post('api/ssbh/OrganizationUser/Update', { mainData: updateData })
                .then(res => {
                    uni.hideLoading();
                    if (res.status || res.success) {
                        uni.showToast({ title: '设置成功', icon: 'success' });
                        this.showPositionModal = false;
                        this.fetchDeptMembers(); // Refresh list
                    } else {
                        uni.showToast({ title: res.message || '设置失败', icon: 'none' });
                    }
                })
                .catch(err => {
                    uni.hideLoading();
                    uni.showToast({ title: 'Error', icon: 'none' });
                });
        }
    }
}
</script>

<style lang="scss" scoped>
/* Styles copied from organization.vue */
.container {
    display: flex;
    flex-direction: column;
    height: 100vh;
    background-color: $sull-color-background;
}

.header {
    background: #fff;
    box-shadow: 0 4rpx 20rpx rgba(0, 0, 0, 0.02);
    position: relative;
    z-index: 100;
}

.header-content {
    height: 88rpx;
    display: flex;
    align-items: center;
    padding: 0 48rpx;
    position: relative;
}

.header-back-btn {
    position: absolute;
    left: 48rpx;
    z-index: 10;
}

.header-title {
    font-size: 34rpx;
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
    gap: 48rpx;
    padding: 48rpx;
}

.info-text {
    padding: 32rpx;
    background: #E6F7FF;
    border-radius: 16rpx;
    color: #1890FF;
    font-size: 28rpx;
    margin-bottom: 16rpx;
}

.form-group {
    display: flex;
    flex-direction: column;
    gap: 16rpx;
}

.label {
    font-size: 28rpx;
    color: $sull-text-secondary;
    font-weight: 500;
}

.input {
    height: 100rpx;
    background: #fff;
    border-radius: 24rpx;
    padding: 0 32rpx;
    font-size: 28rpx;
    color: $sull-text-main;
    width: 100%;
    box-sizing: border-box;
}

.input.readonly {
    background: #f5f5f5;
    color: #999;
}

.textarea {
    height: 200rpx;
    background: #fff;
    border-radius: 24rpx;
    padding: 32rpx;
    font-size: 28rpx;
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
    padding: 12rpx 32rpx;
    border-radius: 40rpx;
    font-size: 28rpx;
    font-weight: 500;
    align-self: flex-start;
}

.status-badge-small {
    display: inline-flex;
    padding: 4rpx 16rpx;
    border-radius: 8rpx;
    font-size: 24rpx;
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
    margin-top: 48rpx;
    margin-bottom: 80rpx;
    display: flex;
    flex-direction: column;
    gap: 32rpx;
}

.btn-primary {
    width: 100%;
    height: 100rpx;
    background: $sull-color-primary;
    color: #fff;
    border-radius: 50rpx;
    font-size: 32rpx;
    font-weight: 600;
    display: flex;
    align-items: center;
    justify-content: center;
}

.btn-danger {
    width: 100%;
    height: 100rpx;
    background: #FFF1F0;
    color: #F5222D;
    border-radius: 50rpx;
    font-size: 32rpx;
    font-weight: 600;
    display: flex;
    align-items: center;
    justify-content: center;
    border: 2rpx solid #FFCCC7;
}

/* List Views */
.list-container {
    padding: 40rpx;
    display: flex;
    flex-direction: column;
    gap: 40rpx;
}

.section-header {
    margin-bottom: 16rpx;
}

.section-title {
    font-size: 36rpx;
    font-weight: bold;
    color: $sull-text-main;
}

.dept-list {
    display: flex;
    flex-direction: column;
    gap: 32rpx;
}

.dept-card {
    background: #fff;
    padding: 32rpx;
    border-radius: 24rpx;
    display: flex;
    justify-content: space-between;
    align-items: center;
    box-shadow: 0 4rpx 16rpx rgba(0, 0, 0, 0.02);
}

.clickable:active {
    background: #fafafa;
}

.dept-info {
    display: flex;
    flex-direction: column;
    gap: 8rpx;
}

.dept-name {
    font-size: 32rpx;
    font-weight: 600;
    color: $sull-text-main;
}

.dept-sub {
    font-size: 24rpx;
    color: $sull-text-secondary;
}

.btn-join {
    background: #E6F7FF;
    color: #1890FF;
    font-size: 24rpx;
    padding: 12rpx 32rpx;
    border-radius: 32rpx;
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
    width: 32rpx;
    height: 32rpx;
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
    width: 28rpx;
    height: 28rpx;
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
    gap: 8rpx;
    margin-top: 8rpx;
}

.empty-state {
    padding: 40rpx;
    text-align: center;
    color: $sull-text-secondary;
    font-size: 28rpx;
    background: #fff;
    border-radius: 24rpx;
}

.loading-state {
    display: flex;
    justify-content: center;
    padding-top: 80rpx;
    color: $sull-text-secondary;
}



/* View Mode Styles */
.view-mode {
    display: flex;
    flex-direction: column;
    gap: 32rpx;
}

.info-card {
    background: #fff;
    border-radius: 32rpx;
    padding: 40rpx;
    display: flex;
    flex-direction: column;
    gap: 32rpx;
}

.info-row {
    display: flex;
    justify-content: space-between;
    align-items: center;
}

.info-col {
    display: flex;
    flex-direction: column;
    gap: 16rpx;
}

.info-label {
    font-size: 28rpx;
    color: $sull-text-secondary;
}

.info-value {
    font-size: 28rpx;
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
    gap: 48rpx;
}

/* Grid Styles */
.section-card {
    background: #fff;
    border-radius: 32rpx;
    padding: 32rpx;
}

.dept-grid {
    display: flex;
    flex-wrap: wrap;
    gap: 24rpx;
}

.dept-grid-item {
    /* 3 items per row minus gaps: (100% - (2 * 12px)) / 3 */
    width: calc((100% - 48rpx) / 3);
    background: #F5F7FA;
    border-radius: 24rpx;
    padding: 24rpx 16rpx;
    display: flex;
    flex-direction: column;
    align-items: center;
    gap: 16rpx;
    box-sizing: border-box;
    border: 2rpx solid transparent;
    transition: all 0.3s ease;
}

.dept-grid-item.selected-item {
    background: #EBF3FF;
    border-color: $sull-color-primary;
}

.dept-grid-item.joined-item {
    /* Optional: subtle hint for joined dept */
}

.value-with-action {
    display: flex;
    align-items: center;
    gap: 16rpx;
}

.joined-badge {
    background: rgba(82, 196, 26, 0.1);
    padding: 4rpx 12rpx;
    border-radius: 8rpx;
}

.joined-text {
    font-size: 20rpx;
    color: #52C41A;
    font-weight: bold;
}

.dept-info-center {
    display: flex;
    flex-direction: column;
    align-items: center;
    gap: 8rpx;
    width: 100%;
}

.dept-name-small {
    font-size: 28rpx;
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
    border-radius: 16rpx;
    padding: 16rpx 24rpx;
    display: flex;
    align-items: center;
    gap: 16rpx;
    margin-bottom: 32rpx;
}

.icon-search {
    width: 32rpx;
    height: 32rpx;
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
    font-size: 28rpx;
    color: $sull-text-main;
    flex: 1;
    background: transparent;
    border: none;
    outline: none;
    height: 48rpx;
    line-height: 48rpx;
}

.dept-grid-scroll {
    height: 404rpx;
    overflow-y: auto;
}

.dept-duty-row-center {
    display: flex;
    align-items: center;
    justify-content: center;
    gap: 8rpx;
    width: 100%;
}

.dept-sub-small {
    font-size: 22rpx;
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
    font-size: 22rpx;
    padding: 0;
    height: 48rpx;
    line-height: 48rpx;
    width: 80%;
    border-radius: 24rpx;
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
    border: 2rpx solid #91D5FF;
}

.joined-text {
    font-size: 22rpx;
    color: $sull-text-secondary;
    font-weight: 500;
}

/* Override empty state if needed or reuse */

/* Tabs */
.tabs {
    display: flex;
    background: #fff;
    border-radius: 50rpx;
    padding: 8rpx;
    margin-bottom: 40rpx;
    box-shadow: 0 4rpx 16rpx rgba(0, 0, 0, 0.02);
}

.tab-item {
    flex: 1;
    height: 84rpx;
    display: flex;
    align-items: center;
    justify-content: center;
    font-size: 28rpx;
    color: $sull-text-secondary;
    font-weight: 500;
    border-radius: 42rpx;
    transition: all 0.3s;
}

.tab-item.active {
    background: $sull-color-primary;
    /* Using a lighter blue similar to image or fallback to primary if variable not perfect match */
    color: #fff;
    font-weight: 600;
    box-shadow: 0 4rpx 12rpx rgba(149, 202, 255, 0.4);
}

/* Removed tab-indicator styles */

/* Members List */
.members-container {
    display: flex;
    flex-direction: column;
    gap: 24rpx;
}

.member-card {
    background: #fff;
    border-radius: 24rpx;
    padding: 32rpx;
    display: flex;
    justify-content: space-between;
    align-items: center;
    box-shadow: 0 2rpx 8rpx rgba(0, 0, 0, 0.02);
}

.member-info {
    display: flex;
    flex-direction: column;
    gap: 12rpx;
}

.member-main {
    display: flex;
    align-items: center;
    gap: 16rpx;
}

.member-name {
    font-size: 30rpx;
    font-weight: 600;
    color: $sull-text-main;
}

.member-title {
    font-size: 22rpx;
    background: #F0F5FF;
    color: #409EFF;
    padding: 4rpx 16rpx;
    border-radius: 8rpx;
    margin-left: 8rpx;
}

.duty-badge {
    display: flex;
    align-items: center;
    gap: 8rpx;
    background: #FFF7E6;
    color: #FA8C16;
    padding: 4rpx 16rpx;
    border-radius: 8rpx;
    font-size: 22rpx;
}

.icon-duty {
    width: 24rpx;
    height: 24rpx;
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
    font-size: 24rpx;
    color: $sull-text-secondary;
}

.btn-set-duty {
    background: #E6F7FF;
    color: #1890FF;
    font-size: 22rpx;
    padding: 0 24rpx;
    height: 48rpx;
    line-height: 48rpx;
    border-radius: 24rpx;
    border: none;
}

.btn-set-duty:active {
    background: #D6EFFF;
}

.btn-set-duty::after {
    border: none;
}

/* Dots Button */
.dots-btn {
    width: 60rpx;
    height: 60rpx;
    display: flex;
    flex-direction: column;
    justify-content: center;
    align-items: center;
    gap: 6rpx;
}

.dots-btn .dot {
    width: 8rpx;
    height: 8rpx;
    background-color: #999;
    border-radius: 50%;
}

/* Action Sheet */
.action-sheet-mask {
    position: fixed;
    top: 0;
    left: 0;
    right: 0;
    bottom: 0;
    background: rgba(0, 0, 0, 0.5);
    z-index: 1000;
    display: flex;
    flex-direction: column;
    justify-content: flex-end;
}

.action-sheet {
    background: #f8f8f8;
    border-radius: 32rpx 32rpx 0 0;
    overflow: hidden;
    animation: slideUp 0.3s ease-out;
}

@keyframes slideUp {
    from {
        transform: translateY(100%);
    }

    to {
        transform: translateY(0);
    }
}

.action-header {
    padding: 32rpx;
    background: #fff;
    text-align: center;
    border-bottom: 2rpx solid #eee;
}

.action-title {
    font-size: 28rpx;
    color: #999;
}

.action-list {
    background: #fff;
}

.action-item {
    height: 100rpx;
    display: flex;
    align-items: center;
    justify-content: center;
    font-size: 32rpx;
    color: #333;
    border-bottom: 2rpx solid #eee;
}

.action-item:active {
    background: #fafafa;
}

.action-cancel {
    margin-top: 16rpx;
    height: 100rpx;
    background: #fff;
    display: flex;
    align-items: center;
    justify-content: center;
    font-size: 32rpx;
    color: #666;
}

/* Position Modal */
.position-modal {
    width: 90% !important;
    max-height: 80vh;
}

.level-row {
    display: flex;
    flex-wrap: wrap;
    gap: 16rpx;
    padding: 24rpx 0;
    border-bottom: 2rpx solid #f0f0f0;
}

.position-tag {
    padding: 12rpx 24rpx;
    background: #f5f5f5;
    border-radius: 8rpx;
    font-size: 26rpx;
    color: #666;
    border: 2rpx solid transparent;
}

.position-tag.active {
    background: rgba($sull-color-primary, 0.1);
    color: $sull-color-primary;
    border-color: $sull-color-primary;
}

/* Modal Base Styles */
.modal-mask {
    position: fixed;
    top: 0;
    left: 0;
    right: 0;
    bottom: 0;
    background: rgba(0, 0, 0, 0.6);
    display: flex;
    align-items: center;
    justify-content: center;
    z-index: 2000;
}

.modal-content {
    background: #fff;
    border-radius: 32rpx;
    padding: 40rpx;
    display: flex;
    flex-direction: column;
    animation: popIn 0.3s cubic-bezier(0.175, 0.885, 0.32, 1.275);
}

@keyframes popIn {
    from {
        opacity: 0;
        transform: scale(0.9);
    }

    to {
        opacity: 1;
        transform: scale(1);
    }
}

.modal-header {
    display: flex;
    justify-content: space-between;
    align-items: center;
    margin-bottom: 32rpx;
}

.modal-title {
    font-size: 32rpx;
    font-weight: bold;
    color: $sull-text-main;
}

.close-btn {
    font-size: 40rpx;
    color: #ccc;
    padding: 10rpx;
}

.modal-body {
    padding: 16rpx 0;
}

.modal-footer {
    margin-top: 48rpx;
}
</style>
