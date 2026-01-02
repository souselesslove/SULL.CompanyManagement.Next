<template>
    <view class="container" @click="activeActionItemId = null">
        <!-- Header -->
        <!-- Header (Adapted from Handover Style) -->
        <view class="custom-nav" :style="{ paddingTop: statusBarHeight * 2 + 'rpx' }">
            <view class="nav-content">
                <view class="nav-left">
                    <BackButton class="nav-back-btn" @click="goBack" />
                </view>
                <text class="nav-title">{{ $t('schedule.title') || '工作排班' }}</text>
                <view class="nav-right"></view>
            </view>
        </view>

        <scroll-view scroll-y class="content" refresher-enabled="true" :refresher-triggered="isRefreshing"
            :refresher-threshold="60" refresher-default-style="none" refresher-background="#f0f5ff"
            @refresherrefresh="onRefresh" @refresherpulling="onPulling" @refresherrestore="onRestore">

            <view slot="refresher" class="custom-refresher"
                :style="{ transform: (isPulling || isRefreshing) ? 'translateY(120rpx)' : 'translateY(0)' }">
                <view class="refresher-content">
                    <view class="refresher-icon" :class="{ 'animating': isRefreshing }"></view>
                    <text class="refresher-text">{{ isRefreshing ? $t('index.refreshing') : $t('index.pullToRefresh')
                        }}</text>
                </view>
            </view>
            <!-- Tabs -->
            <CapsuleTabs :tabs="tabList" v-model:current="currentTab"
                containerStyle="padding-left: 32rpx; padding-right: 32rpx; margin-bottom: 24rpx;" />

            <view class="inner-content">
                <!-- Tab 0: Schedule Plan (Matrix View) - Restricted -->
                <view v-if="currentTab === 0" class="tab-content">
                    <!-- Row 1: Actions & Date Nav -->
                    <view class="schedule-header">
                        <view class="left-actions">
                            <button class="btn-primary-icon" @click="showAddSchedule = true" v-if="canManage">
                                <text class="plus-icon">+</text> {{ $t('schedule.add_button') }}
                            </button>
                        </view>
                        <view class="date-switcher">
                            <view class="icon-btn" @click="changeWeek(-1)">◀</view>
                            <text class="current-date">{{ formatDate(currentWeekStart) }}</text>
                            <view class="icon-btn" @click="changeWeek(1)">▶</view>
                        </view>
                    </view>

                    <!-- Row 2: Matrix -->
                    <view class="schedule-matrix">
                        <!-- Matrix Header -->
                        <view class="matrix-row header">
                            <view class="matrix-cell name-col" @click="toggleSelectAll" v-if="canManage">
                                <text>{{ $t('schedule.name_header') }}</text>
                                <text class="sub-text">({{ selectedUsers.length }}/{{ members.length }})</text>
                            </view>
                            <view class="matrix-cell name-col" v-else>
                                <text>{{ $t('schedule.name_header') }}</text>
                            </view>
                            <view class="matrix-cell date-col" v-for="(date, i) in weekDates" :key="i">
                                <text class="week-day">{{ getWeekDayName(i) }}</text>
                                <text class="day-date">{{ formatDateShort(date) }}</text>
                            </view>
                        </view>

                        <!-- Matrix Body -->
                        <view class="matrix-body">
                            <view class="matrix-row" v-for="(user, uIdx) in matrixData" :key="uIdx">
                                <view class="matrix-cell name-col" @click="toggleUser(user.UserId)"
                                    @longpress="showUserMatrixPreview(user)" v-if="canManage">
                                    <checkbox :checked="selectedUsers.some(uid => String(uid) == String(user.UserId))"
                                        color="#009A97" @click.stop="toggleUser(user.UserId)"
                                        style="transform:scale(0.6)" />
                                    <text class="user-name"
                                        :class="{ 'small-name': user.UserName && user.UserName.length > 3 }">{{
                                            user.UserName }}</text>
                                </view>
                                <view class="matrix-cell name-col" @longpress="showUserMatrixPreview(user)" v-else>
                                    <text class="user-name"
                                        :class="{ 'small-name': user.UserName && user.UserName.length > 3 }">{{
                                            user.UserName }}</text>
                                </view>
                                <view class="matrix-cell date-col" v-for="(day, dIdx) in user.weekShifts" :key="dIdx">
                                    <view v-if="day" class="shift-tag" :class="getShiftClass(day.ShiftTypeName, day)">
                                        {{ day.ShiftTypeName ? day.ShiftTypeName[0] : '' }}
                                    </view>
                                </view>
                            </view>
                            <view v-if="matrixData.length === 0" class="empty-state">
                                <image class="empty-img" src="/static/empty.png" mode="aspectFit" />
                                <text class="empty-text">{{ $t('schedule.empty_schedule') }}</text>
                            </view>
                        </view>
                    </view>
                </view>

                <!-- Tab 1: My Schedule (Calendar View) -->
                <view v-if="currentTab === 1" class="tab-content">
                    <view class="calendar-header">
                        <view class="month-switcher">
                            <view class="icon-btn" @click="changeMonth(-1)">◀</view>
                            <text class="current-month">{{ currentMonth }}</text>
                            <view class="icon-btn" @click="changeMonth(1)">▶</view>
                        </view>
                    </view>

                    <view class="calendar-grid">
                        <view class="calendar-weekdays">
                            <text v-for="d in calendarWeekLabels" :key="d">{{ d }}</text>
                        </view>
                        <view class="calendar-body">
                            <view v-for="(day, idx) in calendarDays" :key="idx" class="calendar-day"
                                :class="{ 'other-month': !day.isCurrentMonth }">
                                <text class="day-num">{{ day.day }}</text>
                                <view v-if="day.shift" class="calendar-shift-tag"
                                    :class="getShiftClass(day.shift.ShiftTypeName, day.shift)">
                                    {{ day.shift.ShiftTypeName }}
                                </view>
                            </view>
                        </view>
                    </view>
                </view>

                <!-- Tab 2: Shift Leave -->
                <view v-if="currentTab === 2" class="tab-content">
                    <view class="action-bar">
                        <button class="btn-primary" @click="showAddLeave = true">{{ $t('schedule.apply_leave')
                        }}</button>
                    </view>
                    <view class="list-container">
                        <view v-for="(item, index) in leaveList" :key="index" class="card">
                            <view class="card-header">
                                <view class="header-left">
                                    <text class="user-name">{{ item.Creator || item.UserName || getUserName(item.UserId)
                                    }}</text>
                                </view>
                                <view class="header-right">
                                    <view class="status-tag" :class="getAuditStatusClass(item.AuditStatus)">
                                        {{ getAuditStatusText(item.AuditStatus) }}
                                    </view>
                                    <view class="action-menu"
                                        v-if="(canManage || item.UserId === userInfo.Id) && item.AuditStatus === 0"
                                        @click.stop="toggleActionMenu(item, 'leave')">
                                        <text class="menu-icon">⋮</text>
                                        <view class="action-dropdown"
                                            v-if="activeActionItemId === item.Id && activeActionItemType === 'leave'">
                                            <view class="action-item"
                                                @click.stop="auditLeave(item, 1); activeActionItemId = null"
                                                v-if="canManage">
                                                <text class="action-icon">✅</text>
                                                <text class="action-text">{{ $t('schedule.approve') }}</text>
                                            </view>
                                            <view class="action-item"
                                                @click.stop="auditLeave(item, 2); activeActionItemId = null"
                                                v-if="canManage">
                                                <text class="action-icon">❌</text>
                                                <text class="action-text">{{ $t('schedule.reject') }}</text>
                                            </view>
                                            <view class="action-item delete-item"
                                                @click.stop="deleteLeave(item); activeActionItemId = null"
                                                v-if="item.UserId === userInfo.Id">
                                                <text class="action-icon">🗑️</text>
                                                <text class="action-text">{{ $t('schedule.delete') }}</text>
                                            </view>
                                        </view>
                                    </view>
                                </view>
                            </view>
                            <view class="card-body">
                                <view class="row"><text class="label">{{ $t('schedule.start') }}:</text> <text>{{
                                    formatDate(item.StartDate)
                                        }}</text></view>
                                <view class="row"><text class="label">{{ $t('schedule.end') }}:</text> <text>{{
                                    formatDate(item.EndDate)
                                        }}</text>
                                </view>
                                <view class="row" v-if="item.Remark"><text class="label">{{ $t('schedule.reason')
                                }}:</text> <text>{{
                                            item.Remark
                                        }}</text></view>
                            </view>
                        </view>
                        <view v-if="leaveList.length === 0" class="empty-state">
                            <image class="empty-img" src="/static/empty.png" mode="aspectFit" />
                            <text class="empty-text">{{ $t('schedule.empty_leave') }}</text>
                        </view>
                    </view>
                </view>

                <!-- Tab 3: Shift Types (Manage) -->
                <view v-if="currentTab === 3" class="tab-content">
                    <view class="action-bar header-actions">
                        <button class="btn-primary" @click="openTypeModal()">{{ $t('schedule.add_type') }}</button>
                    </view>
                    <view class="list-container">
                        <view v-for="(item, index) in typeList" :key="index" class="card">
                            <view class="card-header">
                                <view class="header-left">
                                    <text class="title">{{ item.Title }}</text>
                                    <text class="type-tag" v-if="false">{{ getCategoryText(item.Category) }}</text>
                                </view>
                                <view class="action-menu" v-if="canManage" @click.stop="toggleActionMenu(item, 'type')">
                                    <text class="menu-icon">⋮</text>
                                    <view class="action-dropdown"
                                        v-if="activeActionItemId === item.Id && activeActionItemType === 'type'">
                                        <view class="action-item"
                                            @click.stop="openTypeModal(item); activeActionItemId = null">
                                            <text class="action-icon">✏️</text>
                                            <text class="action-text">{{ $t('common.edit') }}</text>
                                        </view>
                                        <view class="action-item delete-item"
                                            @click.stop="deleteType(item); activeActionItemId = null">
                                            <text class="action-icon">🗑️</text>
                                            <text class="action-text">{{ $t('common.delete') }}</text>
                                        </view>
                                    </view>
                                </view>
                            </view>
                            <view class="card-body">
                                <text>{{ $t('schedule.duration') }}: {{ item.Duration }}{{ $t('schedule.duration_unit')
                                }}</text>
                                <text>{{ formatTimeString(item.StartTime) }} - {{ formatTimeString(item.EndTime)
                                    }}</text>
                                <text class="desc" v-if="item.Description">{{ item.Description }}</text>
                            </view>
                        </view>
                        <view v-if="typeList.length === 0" class="empty-state">
                            <image class="empty-img" src="/static/empty.png" mode="aspectFit" />
                            <text class="empty-text">{{ $t('schedule.empty_type') }}</text>
                        </view>
                    </view>
                </view>
            </view>

            <FooterInfo />
        </scroll-view>

        <!-- User Monthly Preview Modal -->
        <view class="modal-overlay" v-if="showUserPreview" @click="showUserPreview = false">
            <view class="modal-content calendar-modal" @click.stop>
                <view class="modal-header">
                    <text class="modal-title">{{ previewUser ? previewUser.UserName : '' }}</text>
                    <text class="close-btn" @click="showUserPreview = false">✖️</text>
                </view>
                <view class="modal-body" style="padding: 24rpx;">
                    <view class="calendar-header" style="padding: 0 0 24rpx 0;">
                        <view class="month-switcher">
                            <view class="icon-btn" @click="changeMonth(-1)">◀</view>
                            <text class="current-month">{{ currentMonth }}</text>
                            <view class="icon-btn" @click="changeMonth(1)">▶</view>
                        </view>
                    </view>
                    <view class="calendar-grid">
                        <view class="calendar-weekdays">
                            <text v-for="d in calendarWeekLabels" :key="d">{{ d }}</text>
                        </view>
                        <view class="calendar-body">
                            <view v-for="(day, idx) in previewCalendarDays" :key="idx" class="calendar-day"
                                :class="{ 'other-month': !day.isCurrentMonth }">
                                <text class="day-num">{{ day.day }}</text>
                                <view v-if="day.shift" class="calendar-shift-tag"
                                    :class="getShiftClass(day.shift.ShiftTypeName, day.shift)">
                                    {{ day.shift.ShiftTypeName }}
                                </view>
                            </view>
                        </view>
                    </view>
                </view>
            </view>
        </view>

        <!-- Modals -->
        <!-- Add Schedule Modal -->
        <!-- Modals (Handover Style) -->
        <!-- Add Schedule Modal -->
        <view class="modal-overlay" v-if="showAddSchedule" @click="showAddSchedule = false">
            <view class="modal-content" @click.stop>
                <view class="modal-header">
                    <text class="modal-title">{{ $t('schedule.add_button') }}</text>
                    <text class="close-btn" @click="showAddSchedule = false">✖️</text>
                </view>
                <view class="modal-body form">
                    <picker mode="selector" :range="members" range-key="UserName" @change="onMemberChange"
                        v-if="!selectedUsers.length">
                        <view class="input-item">
                            <text>{{ $t('schedule.name') }}: {{ newSchedule.UserName || $t('schedule.please_select')
                            }}</text>
                        </view>
                    </picker>
                    <view class="input-item" v-else>
                        <text>{{ $t('schedule.selected_users_count', { count: selectedUsers.length }) }}</text>
                    </view>
                    <picker mode="date" @change="e => newSchedule.StartDate = e.detail.value"
                        :value="newSchedule.StartDate || currentWeekStart">
                        <view class="input-item">{{ $t('schedule.start_date') }}: {{ newSchedule.StartDate ||
                            $t('schedule.please_select') }}</view>
                    </picker>
                    <picker mode="date" @change="e => newSchedule.EndDate = e.detail.value"
                        :value="newSchedule.EndDate || currentWeekStart">
                        <view class="input-item">{{ $t('schedule.end_date') }}: {{ newSchedule.EndDate ||
                            $t('schedule.please_select') }}</view>
                    </picker>
                    <picker mode="selector" :range="typeList" range-key="Title" @change="onShiftTypeSelect">
                        <view class="input-item">
                            <text>{{ $t('schedule.shift') }}: {{ newSchedule.ShiftTypeName ||
                                $t('schedule.please_select')
                            }}</text>
                        </view>
                    </picker>
                    <input class="input-item" :placeholder="$t('schedule.remark')" v-model="newSchedule.Remark" />
                </view>
                <view class="modal-footer">
                    <button class="modal-btn cancel" @click="showAddSchedule = false">{{ $t('common.cancel') }}</button>
                    <button class="modal-btn confirm" @click="submitSchedule">{{ $t('common.confirm') }}</button>
                </view>
            </view>
        </view>

        <!-- Add Type Modal -->
        <view class="modal-overlay" v-if="showTypeModal" @click="showTypeModal = false">
            <view class="modal-content" @click.stop>
                <view class="modal-header">
                    <text class="modal-title">{{ editingType.Id ? $t('schedule.edit_type') : $t('schedule.add_type')
                    }}</text>
                    <text class="close-btn" @click="showTypeModal = false">✖️</text>
                </view>
                <view class="modal-body form">
                    <input class="input-item" :placeholder="$t('schedule.name')" v-model="editingType.Title" />
                    <view class="row-center">
                        <text>{{ $t('schedule.type') }}:</text>
                        <picker mode="selector"
                            :range="[$t('schedule.type_normal'), $t('schedule.type_night'), $t('schedule.type_other')]"
                            @change="onCatChange">
                            <view class="picker-box">{{ getCategoryText(editingType.Category) ||
                                $t('schedule.please_select') }}
                            </view>
                        </picker>
                    </view>
                    <view class="row-center">
                        <text>{{ $t('schedule.time') }}:</text>
                        <picker mode="time" :value="editingType.StartTime"
                            @change="e => editingType.StartTime = e.detail.value">
                            <view class="time-box">{{ formatTimeString(editingType.StartTime) || '00:00' }}</view>
                        </picker>
                        <text>-</text>
                        <picker mode="time" :value="editingType.EndTime"
                            @change="e => editingType.EndTime = e.detail.value">
                            <view class="time-box">{{ formatTimeString(editingType.EndTime) || '00:00' }}</view>
                        </picker>
                    </view>

                    <input class="input-item" :placeholder="$t('schedule.description')"
                        v-model="editingType.Description" />
                </view>
                <view class="modal-footer">
                    <!-- If adding, show APN button in 'cancel' slot (white button) -->
                    <button class="modal-btn cancel" v-if="!editingType.Id" @click="generateAPNShifts">{{
                        $t('schedule.apn_button') }}</button>
                    <button class="modal-btn cancel" v-else @click="showTypeModal = false">{{ $t('common.cancel')
                    }}</button>
                    <button class="modal-btn confirm" @click="submitType">{{ $t('common.save') }}</button>
                </view>
            </view>
        </view>

        <!-- Leave Modal -->
        <view class="modal-overlay" v-if="showAddLeave" @click="showAddLeave = false">
            <view class="modal-content" @click.stop>
                <view class="modal-header">
                    <text class="modal-title">{{ $t('schedule.apply_leave') }}</text>
                    <text class="close-btn" @click="showAddLeave = false">✖️</text>
                </view>
                <view class="modal-body form">
                    <picker mode="date" @change="e => newLeave.StartDate = e.detail.value">
                        <view class="input-item">{{ $t('schedule.start') }}: {{ newLeave.StartDate ||
                            $t('schedule.please_select') }}</view>
                    </picker>
                    <picker mode="date" @change="e => newLeave.EndDate = e.detail.value">
                        <view class="input-item">{{ $t('schedule.end') }}: {{ newLeave.EndDate ||
                            $t('schedule.please_select')
                        }}</view>
                    </picker>
                    <textarea class="textarea" :placeholder="$t('schedule.leave_reason')"
                        v-model="newLeave.Remark"></textarea>
                </view>
                <view class="modal-footer">
                    <button class="modal-btn cancel" @click="showAddLeave = false">{{ $t('common.cancel') }}</button>
                    <button class="modal-btn confirm" @click="submitLeave">{{ $t('common.confirm') }}</button>
                </view>
            </view>
        </view>

    </view>
</template>

<script>
import request from '@/utils/request.js';
import { storage } from '@/utils/storage.js';
import CapsuleTabs from '@/components/CapsuleTabs/CapsuleTabs.vue';
import dayjs from 'dayjs';
import isBetween from 'dayjs/plugin/isBetween';

dayjs.extend(isBetween);

import FooterInfo from '@/components/FooterInfo/FooterInfo.vue';
import BackButton from '@/components/BackButton/BackButton.vue';

export default {
    components: {
        CapsuleTabs,
        FooterInfo,
        BackButton
    },
    data() {
        return {
            statusBarHeight: 20,
            currentTab: 0,
            isRefreshing: false,
            isPulling: false,
            currentWeekStart: '', // Calculated in mounted
            weekDates: [], // Array of 7 date strings
            scheduleList: [],
            leaveList: [],
            typeList: [],
            members: [],
            selectedUsers: [], // Array of UserIds

            // Modals
            showAddSchedule: false,
            showTypeModal: false,
            showAddLeave: false,

            // Forms
            newSchedule: { UserId: null, UserName: '', ShiftTypeId: null, ShiftTypeName: '', Remark: '', StartDate: '', EndDate: '' },
            editingType: { Id: 0, Title: '', Category: 0, StartTime: '', EndTime: '', Duration: 480, Description: '' },
            newLeave: { StartDate: '', EndDate: '', Remark: '' },

            userInfo: {},
            userOrgInfo: {},
            activeActionItemId: null,
            activeActionItemType: '', // 'leave' or 'type'
            currentMonth: '', // YYYY-MM
            personalSchedules: [],

            // User Preview
            showUserPreview: false,
            previewUser: null,
            previewSchedules: [],
            tabLoadedStatus: { 0: false, 1: false, 2: false, 3: false }
        }
    },
    onLoad() {
        this.userInfo = storage.get('userInfo') || {};
        this.userOrgInfo = storage.get('userOrgInfo') || {};

        const d = new Date();
        this.currentMonth = d.getFullYear() + '-' + String(d.getMonth() + 1).padStart(2, '0');

        // Initial tab: If not manager, default to My Schedule (Tab 1)
        if (!this.canManage) {
            this.currentTab = 1;
        } else {
            this.currentTab = 0; // Default to Matrix for managers
        }

        this.loadAll();
    },
    watch: {
        currentTab(newVal) {
            this.loadTabData(newVal);
        }
    },
    mounted() {
        const sysInfo = uni.getSystemInfoSync();
        this.statusBarHeight = sysInfo.statusBarHeight || 20;
    },
    computed: {
        canManage() {
            // Only level >= 30
            return (this.userOrgInfo.User?.Level || 0) >= 30;
        },
        isChinese() {
            return this.$i18n.locale === 'zh-Hans';
        },
        calendarWeekLabels() {
            if (this.isChinese) {
                return [
                    this.$t('schedule.day_mon'),
                    this.$t('schedule.day_tue'),
                    this.$t('schedule.day_wed'),
                    this.$t('schedule.day_thu'),
                    this.$t('schedule.day_fri'),
                    this.$t('schedule.day_sat'),
                    this.$t('schedule.day_sun')
                ];
            } else {
                return [
                    this.$t('schedule.day_sun'),
                    this.$t('schedule.day_mon'),
                    this.$t('schedule.day_tue'),
                    this.$t('schedule.day_wed'),
                    this.$t('schedule.day_thu'),
                    this.$t('schedule.day_fri'),
                    this.$t('schedule.day_sat')
                ];
            }
        },
        matrixData() {
            // Explicitly depend on reactive sources for correct updates
            const members = this.members;
            const weekDates = this.weekDates;
            const leaves = this.leaveList;

            if (!members.length || !weekDates.length) return [];

            return members.map(member => {
                const row = {
                    UserId: member.UserId,
                    UserName: member.UserName,
                    weekShifts: new Array(7).fill(null)
                };

                const sid = String(member.UserId);
                this.weekDates.forEach((dateStr, i) => {
                    const target = dayjs(dateStr).startOf('day');

                    // 1. Prioritize Leave
                    const leave = leaves.find(l => {
                        const leaveUid = l.UserId || l.CreatorId;
                        if (String(leaveUid) !== sid) return false;
                        // Include pending (0) or approved (1)
                        if (l.AuditStatus !== 1 && l.AuditStatus !== 0) return false;

                        const start = dayjs(l.StartDate).startOf('day');
                        const end = dayjs(l.EndDate).startOf('day');
                        return target.isBetween(start, end, 'day', '[]');
                    });

                    if (leave) {
                        row.weekShifts[i] = {
                            ShiftTypeName: this.$t('schedule.leave_label') || '休假',
                            isLeave: true
                        };
                    } else {
                        // 2. Schedule
                        const shift = this.scheduleList.find(s => {
                            const shiftUid = s.UserId || s.CreatorId;
                            if (String(shiftUid) !== sid) return false;
                            return dayjs(s.ScheduleDate).startOf('day').isSame(target, 'day');
                        });

                        if (shift) {
                            const type = this.typeList.find(t => String(t.Id) == String(shift.ShiftTypeId));
                            row.weekShifts[i] = {
                                ...shift,
                                ShiftTypeName: type ? type.Title : (shift.ShiftTypeName || '未知'),
                                StartTime: type ? type.StartTime : '',
                                EndTime: type ? type.EndTime : ''
                            };
                        }
                    }
                });

                return row;
            });
        },
        calendarDays() {
            const leaves = this.leaveList; // Reactivity dependency
            if (!this.currentMonth) return [];
            // Use findPersonalShift which calls findShiftInList
            const parts = this.currentMonth.split('-');
            const year = parseInt(parts[0]);
            const month = parseInt(parts[1]) - 1;

            const firstDay = new Date(year, month, 1).getDay();
            const daysInMonth = new Date(year, month + 1, 0).getDate();
            const days = [];

            const prevMonthLastDay = new Date(year, month, 0).getDate();
            for (let i = firstDay - 1; i >= 0; i--) {
                const d = new Date(year, month - 1, prevMonthLastDay - i);
                const fullDate = this.formatDateYMD(d);
                days.push({ day: d.getDate(), fullDate, isCurrentMonth: false, shift: this.findPersonalShift(fullDate) });
            }
            for (let i = 1; i <= daysInMonth; i++) {
                const d = new Date(year, month, i);
                const fullDate = this.formatDateYMD(d);
                days.push({ day: i, fullDate, isCurrentMonth: true, shift: this.findPersonalShift(fullDate) });
            }
            const remaining = 42 - days.length;
            for (let i = 1; i <= remaining; i++) {
                const d = new Date(year, month + 1, i);
                const fullDate = this.formatDateYMD(d);
                days.push({ day: d.getDate(), fullDate, isCurrentMonth: false, shift: this.findPersonalShift(fullDate) });
            }
            return days;
        },
        previewCalendarDays() {
            const leaves = this.leaveList; // Reactivity dependency
            if (!this.currentMonth || !this.previewUser) return [];
            return this.generateMonthDays(this.currentMonth, this.previewSchedules, this.previewUser.UserId);
        },
        tabList() {
            const tabs = [];
            if (this.canManage) {
                tabs.push({ label: 'schedule.tab_schedule_matrix', value: 0 });
            }
            tabs.push({ label: 'schedule.tab_my_schedule', value: 1 });
            tabs.push({ label: 'schedule.tab_leave', value: 2 });
            if (this.canManage) {
                tabs.push({ label: 'schedule.tab_type', value: 3 });
            }
            return tabs;
        }
    },
    methods: {
        goBack() {
            uni.navigateBack();
        },
        onPulling() {
            this.isPulling = true;
        },
        onRestore() {
            this.isPulling = false;
        },
        async onRefresh() {
            this.isRefreshing = true;
            this.isPulling = true;
            this.loadAll();
            setTimeout(() => {
                this.isRefreshing = false;
                this.isPulling = false;
                uni.showToast({ title: '已刷新', icon: 'none' });
            }, 800);
        },
        loadAll() {
            // Reset tab loaded status on refresh/load
            this.tabLoadedStatus = { 0: false, 1: false, 2: false, 3: false };

            // 1. Shared Data (Fetch always as they are small or foundational)
            this.fetchTypes();
            this.fetchMembers();
            this.fetchLeaves(); // Multi-month range leave fetch (shared for all views)

            // 2. Tab Specific Data
            this.loadTabData(this.currentTab);
        },
        loadTabData(tabIndex) {
            if (this.tabLoadedStatus[tabIndex]) return;

            if (tabIndex === 0 && this.canManage) {
                if (!this.currentWeekStart) this.initWeekBase();
                this.fetchSchedule();
                this.tabLoadedStatus[0] = true;
            } else if (tabIndex === 1) {
                this.fetchPersonalMonthly();
                this.tabLoadedStatus[1] = true;
            } else if (tabIndex === 2) {
                // Shared leaves fetched in loadAll
                this.tabLoadedStatus[2] = true;
            } else if (tabIndex === 3) {
                // Shared types fetched in loadAll
                this.tabLoadedStatus[3] = true;
            }
        },
        initWeek() {
            this.initWeekBase();
            this.fetchSchedule();
        },
        initWeekBase() {
            const d = new Date();
            const day = d.getDay();
            const diff = d.getDate() - (day === 0 ? 6 : day - 1);
            const monday = new Date(d.getFullYear(), d.getMonth(), diff);
            this.currentWeekStart = this.formatDateYMD(monday);
            this.updateWeekDates();
        },

        updateWeekDates() {
            const startParts = this.currentWeekStart.split('-');
            const start = new Date(startParts[0], startParts[1] - 1, startParts[2]);
            const dates = [];
            for (let i = 0; i < 7; i++) {
                const d = new Date(start.getFullYear(), start.getMonth(), start.getDate() + i);
                dates.push(this.formatDateYMD(d));
            }
            this.weekDates = dates;
        },

        changeWeek(offset) {
            const startParts = this.currentWeekStart.split('-');
            const d = new Date(startParts[0], startParts[1] - 1, startParts[2]);
            d.setDate(d.getDate() + (offset * 7));
            this.currentWeekStart = this.formatDateYMD(d);
            this.updateWeekDates();
            this.fetchSchedule();
        },
        // --- API Calls ---
        fetchTypes() {
            request.post('api/ssbh/ShiftType/GetPageData', {
                page: 1, rows: 100,
                wheres: JSON.stringify([{ name: 'OrganizationId', value: this.userOrgInfo.Organization?.Id }])
            }).then(res => {
                this.typeList = res.rows || [];
            });
        },
        fetchSchedule() {
            if (!this.currentWeekStart) return;
            // Range: From currentWeekStart 00:00 to next Monday 00:00 (exclusive)
            const startStr = this.currentWeekStart + ' 00:00:00';

            const startParts = this.currentWeekStart.split('-');
            const d = new Date(startParts[0], startParts[1] - 1, startParts[2]);
            d.setDate(d.getDate() + 7); // Next Monday
            const endStr = this.formatDateYMD(d) + ' 00:00:00';

            const wheres = [
                { name: 'OrganizationId', value: this.userOrgInfo.Organization?.Id },
                { name: 'ScheduleDate', value: startStr, displayType: 'thanorequal' },
                { name: 'ScheduleDate', value: endStr, displayType: 'lt' }
            ];

            request.post('api/ssbh/ShiftSchedule/GetPageData', {
                page: 1, rows: 1000,
                wheres: JSON.stringify(wheres)
            }).then(res => {
                this.scheduleList = res.rows || [];
            });
        },
        fetchPersonalMonthly() {
            if (!this.currentMonth) return;
            const parts = this.currentMonth.split('-');
            const year = parseInt(parts[0]);
            const month = parseInt(parts[1]) - 1;

            const startStr = this.formatDateYMD(new Date(year, month, -7)) + ' 00:00:00';
            const endStr = this.formatDateYMD(new Date(year, month + 1, 14)) + ' 00:00:00';

            const wheres = [
                { name: 'OrganizationId', value: this.userOrgInfo.Organization?.Id },
                { name: 'UserId', value: userId },
                { name: 'ScheduleDate', value: startStr, displayType: 'thanorequal' },
                { name: 'ScheduleDate', value: endStr, displayType: 'lt' }
            ];

            request.post('api/ssbh/ShiftSchedule/GetPageData', {
                page: 1, rows: 200,
                wheres: JSON.stringify(wheres)
            }).then(res => {
                this.personalSchedules = res.rows || [];
            });
        },
        fetchUserMonthly(userId) {
            if (!this.currentMonth || !userId) return;
            const parts = this.currentMonth.split('-');
            const year = parseInt(parts[0]);
            const month = parseInt(parts[1]) - 1;

            const startStr = this.formatDateYMD(new Date(year, month, -7)) + ' 00:00:00';
            const endStr = this.formatDateYMD(new Date(year, month + 1, 14)) + ' 00:00:00';

            const wheres = [
                { name: 'OrganizationId', value: this.userOrgInfo.Organization?.Id },
                { name: 'UserId', value: userId },
                { name: 'ScheduleDate', value: startStr, displayType: 'thanorequal' },
                { name: 'ScheduleDate', value: endStr, displayType: 'lt' }
            ];

            request.post('api/ssbh/ShiftSchedule/GetPageData', {
                page: 1, rows: 200,
                wheres: JSON.stringify(wheres)
            }).then(res => {
                this.previewSchedules = res.rows || [];
            });
        },
        generateMonthDays(monthStr, schedules, userId) {
            const parts = monthStr.split('-');
            const year = parseInt(parts[0]);
            const month = parseInt(parts[1]) - 1;

            const firstDay = new Date(year, month, 1).getDay();
            const daysInMonth = new Date(year, month + 1, 0).getDate();
            const days = [];

            // Chinese starts with Monday, others with Sunday
            const padding = this.isChinese ? (firstDay === 0 ? 6 : firstDay - 1) : firstDay;

            const prevMonthLastDay = new Date(year, month, 0).getDate();
            for (let i = padding - 1; i >= 0; i--) {
                const d = new Date(year, month - 1, prevMonthLastDay - i);
                const fullDate = this.formatDateYMD(d);
                days.push({ day: d.getDate(), fullDate, isCurrentMonth: false, shift: this.findShiftInList(fullDate, schedules, userId) });
            }
            for (let i = 1; i <= daysInMonth; i++) {
                const d = new Date(year, month, i);
                const fullDate = this.formatDateYMD(d);
                days.push({ day: i, fullDate, isCurrentMonth: true, shift: this.findShiftInList(fullDate, schedules, userId) });
            }
            const remaining = 42 - days.length;
            for (let i = 1; i <= remaining; i++) {
                const d = new Date(year, month + 1, i);
                const fullDate = this.formatDateYMD(d);
                days.push({ day: d.getDate(), fullDate, isCurrentMonth: false, shift: this.findShiftInList(fullDate, schedules, userId) });
            }
            return days;
        },
        findPersonalShift(date) {
            return this.findShiftInList(date, this.personalSchedules, this.userInfo.Id);
        },
        findShiftInList(date, list, userId) {
            if (!date || !this.leaveList) return null;
            const target = dayjs(date).startOf('day');
            const sid = String(userId);

            // 1. Prioritize Leave
            const leave = this.leaveList.find(l => {
                const leaveUid = l.UserId || l.CreatorId;
                if (String(leaveUid) !== sid) return false;
                // Allow approved (1) or pending (0) for display
                if (l.AuditStatus !== 1 && l.AuditStatus !== 0) return false;

                const start = dayjs(l.StartDate).startOf('day');
                const end = dayjs(l.EndDate).startOf('day');
                return target.isBetween(start, end, 'day', '[]');
            });

            if (leave) {
                return { ShiftTypeName: this.$t('schedule.leave_label') || '休假', isLeave: true };
            }

            // 2. Fallback to Shift
            const shift = list.find(s => {
                if (!s.ScheduleDate) return false;
                return dayjs(s.ScheduleDate).startOf('day').isSame(target, 'day');
            });
            if (shift) {
                const type = this.typeList.find(t => String(t.Id) == String(shift.ShiftTypeId));
                return {
                    ...shift,
                    ShiftTypeName: type ? type.Title : (shift.ShiftTypeName || '未知')
                };
            }
            return null;
        },
        showUserMatrixPreview(user) {
            this.previewUser = user;
            this.previewSchedules = []; // Clear old
            this.fetchUserMonthly(user.UserId);
            this.showUserPreview = true;
        },
        changeMonth(offset) {
            const parts = this.currentMonth.split('-');
            const d = new Date(parseInt(parts[0]), parseInt(parts[1]) - 1 + offset, 1);
            this.currentMonth = d.getFullYear() + '-' + String(d.getMonth() + 1).padStart(2, '0');
            this.fetchPersonalMonthly();
            // fetchLeaves is called inside fetchPersonalMonthly
            if (this.showUserPreview && this.previewUser) {
                this.fetchUserMonthly(this.previewUser.UserId);
            }
        },
        fetchLeaves(sDate, eDate) {
            let startStr = sDate;
            let endStr = eDate;

            // If no range provided, calculate default monthly range
            if (!startStr || !endStr) {
                if (!this.currentMonth) return;
                const parts = this.currentMonth.split('-');
                const year = parseInt(parts[0]);
                const month = parseInt(parts[1]) - 1;
                startStr = this.formatDateYMD(new Date(year, month, -7)) + ' 00:00:00';
                endStr = this.formatDateYMD(new Date(year, month + 1, 14)) + ' 00:00:00';
            }

            const wheres = [
                { name: 'OrganizationId', value: this.userOrgInfo.Organization?.Id },
                { name: 'StartDate', value: endStr, displayType: 'lt' },
                { name: 'EndDate', value: startStr, displayType: 'thanorequal' }
            ];

            request.post('api/ssbh/ShiftLeave/GetPageData', {
                page: 1, rows: 200,
                wheres: JSON.stringify(wheres),
                sort: 'CreationTime', order: 'desc'
            }).then(res => {
                this.leaveList = res.rows || [];
            });
        },
        fetchMembers() {
            request.post('api/ssbh/OrganizationUser/GetPageData', {
                page: 1, rows: 1000,
                wheres: JSON.stringify([{ name: 'OrganizationId', value: this.userOrgInfo.Organization?.Id }])
            }).then(res => {
                this.members = res.rows || [];
            });
        },
        getUserName(userId) {
            if (!userId) return '';
            const member = this.members.find(m => String(m.UserId) === String(userId));
            return member ? member.UserName : '';
        },

        // --- Matrix Helpers ---
        getWeekDayName(idx) {
            // idx is 0-6 (Mon-Sun)
            const days = [
                this.$t('schedule.day_mon'),
                this.$t('schedule.day_tue'),
                this.$t('schedule.day_wed'),
                this.$t('schedule.day_thu'),
                this.$t('schedule.day_fri'),
                this.$t('schedule.day_sat'),
                this.$t('schedule.day_sun')
            ];
            return days[idx];
        },
        formatDateShort(dateStr) {
            if (!dateStr) return '';
            return dateStr.slice(8, 10); // DD
        },
        getShiftClass(name, item) {
            if (!name) return '';
            if (item && item.isLeave) return 'shift-leave';
            const n = name.toUpperCase();
            if (n.includes('早') || n.includes('A')) return 'shift-orange';
            if (n.includes('中') || n.includes('P')) return 'shift-blue';
            if (n.includes('晚') || n.includes('夜') || n.includes('N')) return 'shift-purple';
            return 'shift-default';
        },
        toggleUser(id) {
            if (!this.canManage) return;
            if (!id) return;
            const sid = String(id);
            const idx = this.selectedUsers.findIndex(uid => String(uid) === sid);
            if (idx > -1) {
                this.selectedUsers.splice(idx, 1);
            } else {
                this.selectedUsers.push(id);
            }
            // Force reactivity for some platforms
            this.selectedUsers = [...this.selectedUsers];
        },
        toggleSelectAll() {
            if (!this.canManage) return;
            // Check if all are currently selected (using loose equality for safety)
            const allSelected = this.members.length > 0 && this.members.every(m =>
                this.selectedUsers.some(uid => String(uid) == String(m.UserId))
            );

            if (allSelected) {
                this.selectedUsers = [];
            } else {
                this.selectedUsers = this.members.map(m => m.UserId);
            }
        },

        // --- Handlers ---
        onDateChange(e) {
            // Deprecated for week view, but kept for logic safety if referenced
            // this.selectedDate = e.detail.value;
            // this.fetchSchedule(); // This would now fetch for a single day, not week
        },
        onMemberChange(e) {
            const idx = e.detail.value;
            const m = this.members[idx];
            this.newSchedule.UserId = m.UserId;
            this.newSchedule.UserName = m.UserName;
        },
        onShiftTypeSelect(e) {
            const idx = e.detail.value;
            const t = this.typeList[idx];
            this.newSchedule.ShiftTypeId = t.Id;
            this.newSchedule.ShiftTypeName = t.Title;
        },
        toggleActionMenu(item, type) {
            if (this.activeActionItemId === item.Id && this.activeActionItemType === type) {
                this.activeActionItemId = null;
                this.activeActionItemType = '';
            } else {
                this.activeActionItemId = item.Id;
                this.activeActionItemType = type;
            }
        },
        onCatChange(e) {
            this.editingType.Category = e.detail.value;
        },

        // --- Actions ---
        submitSchedule() {
            if (!this.newSchedule.StartDate || !this.newSchedule.EndDate || (!this.newSchedule.UserId && !this.selectedUsers.length) || !this.newSchedule.ShiftTypeId) {
                uni.showToast({ title: this.$t('schedule.complete_info'), icon: 'none' });
                return;
            }
            const start = new Date(this.newSchedule.StartDate);
            const end = new Date(this.newSchedule.EndDate);
            if (start > end) {
                uni.showToast({ title: this.$t('schedule.date_error'), icon: 'none' });
                return;
            }

            const days = Math.round((end - start) / (1000 * 60 * 60 * 24)) + 1;
            const requests = [];
            const userIds = this.selectedUsers.length ? this.selectedUsers : [this.newSchedule.UserId];

            for (let i = 0; i < days; i++) {
                const d = new Date(start);
                d.setDate(d.getDate() + i);
                const dateStr = this.formatDateYMD(d);

                userIds.forEach(uid => {
                    const member = this.members.find(m => m.UserId === uid);
                    const data = {
                        ...this.newSchedule,
                        UserId: uid,
                        UserName: member ? member.UserName : this.newSchedule.UserName,
                        OrganizationId: this.userOrgInfo.Organization?.Id,
                        DepartmentId: this.userOrgInfo.Department?.Id,
                        ScheduleDate: dateStr
                    };
                    requests.push(request.post('api/ssbh/ShiftSchedule/Add', { mainData: data }));
                });
            }

            Promise.all(requests).then(results => {
                const allSuccess = results.every(res => res.success || res.status);
                if (allSuccess) {
                    this.showAddSchedule = false;
                    this.fetchSchedule();
                    uni.showToast({ title: this.$t('schedule.add_success') });
                    // Clear selection after batch add if desired
                    // this.selectedUsers = [];
                } else {
                    uni.showToast({ title: this.$t('schedule.batch_fail'), icon: 'none' });
                    this.fetchSchedule(); // Refresh to see what succeeded
                }
            }).catch(() => {
                uni.showToast({ title: this.$t('schedule.system_error'), icon: 'none' });
            });
        },
        deleteSchedule(item) {
            uni.showModal({
                title: this.$t('common.tip'), content: this.$t('schedule.confirm_delete'),
                success: (r) => {
                    if (r.confirm) {
                        request.post('api/ssbh/ShiftSchedule/Del', [item.Id]).then(() => {
                            this.fetchSchedule();
                            uni.showToast({ title: this.$t('common.success') });
                        });
                    }
                }
            });
        },

        openTypeModal(item) {
            if (item) {
                this.editingType = { ...item };
            } else {
                this.editingType = { Id: 0, Title: '', Category: 0, StartTime: '', EndTime: '', Duration: 480, Description: '' };
            }
            this.showTypeModal = true;
        },
        submitType() {
            const data = {
                ...this.editingType,
                OrganizationId: this.userOrgInfo.Organization?.Id,
                DepartmentId: this.userOrgInfo.Department?.Id
            };
            const url = data.Id ? 'api/ssbh/ShiftType/Update' : 'api/ssbh/ShiftType/Add';
            data.StartTime = '2000-01-01 ' + (data.StartTime.length === 5 ? data.StartTime + ':00' : data.StartTime); // Dummy date part
            data.EndTime = '2000-01-01 ' + (data.EndTime.length === 5 ? data.EndTime + ':00' : data.EndTime);

            request.post(url, { mainData: data }).then(res => {
                if (res.success || res.status) {
                    this.showTypeModal = false;
                    this.fetchTypes();
                    uni.showToast({ title: this.$t('schedule.save_success') });
                } else {
                    uni.showToast({ title: res.message || this.$t('common.fail'), icon: 'none' });
                }
            });
        },
        deleteType(item) {
            uni.showModal({
                title: this.$t('common.tip'), content: this.$t('schedule.confirm_delete'),
                success: (r) => {
                    if (r.confirm) {
                        request.post('api/ssbh/ShiftType/Del', [item.Id]).then(() => {
                            this.fetchTypes();
                            uni.showToast({ title: this.$t('common.success') });
                        });
                    }
                }
            });
        },

        submitLeave() {
            const data = {
                ...this.newLeave,
                OrganizationId: this.userOrgInfo.Organization?.Id,
                DepartmentId: this.userOrgInfo.Department?.Id,
                UserId: this.userInfo.Id,
                UserName: this.userInfo.TrueName || this.userInfo.UserName,
                AuditStatus: 0,
            };
            request.post('api/ssbh/ShiftLeave/Add', { mainData: data }).then(res => {
                if (res.success || res.status) {
                    this.showAddLeave = false;
                    this.fetchLeaves();
                    uni.showToast({ title: this.$t('schedule.apply_success') });
                } else {
                    uni.showToast({ title: res.message || this.$t('schedule.apply_fail'), icon: 'none' });
                }
            });
        },
        auditLeave(item, status) {
            const data = { ...item, AuditStatus: status, AuditUserId: this.userInfo.Id, AuditUser: this.userInfo.UserName, AuditTime: new Date() };
            const content = status === 1 ? this.$t('schedule.confirm_approve') : this.$t('schedule.confirm_reject');
            uni.showModal({
                title: this.$t('common.tip'),
                content: content,
                success: (res) => {
                    if (res.confirm) {
                        request.post('api/ssbh/ShiftLeave/Update', { mainData: data }).then(() => {
                            uni.showToast({ title: this.$t('common.success') });
                            this.fetchLeaves();
                        });
                    }
                }
            });
        },
        deleteLeave(item) {
            uni.showModal({
                title: this.$t('common.tip'),
                content: this.$t('schedule.confirm_delete_leave'),
                success: (res) => {
                    if (res.confirm) {
                        request.post('api/ssbh/ShiftLeave/Del', [item.Id]).then(() => {
                            uni.showToast({ title: this.$t('schedule.delete_success') });
                            this.fetchLeaves();
                        });
                    }
                }
            });
        },

        generateAPNShifts() {
            uni.showLoading({ title: this.$t('schedule.generating') });
            const params = {
                Category: 10,
                OrganizationId: this.userOrgInfo.Organization?.Id
            };
            request.post('api/ssbh/ShiftType/GenerateDefaultShifts', { mainData: params }).then(res => {
                uni.hideLoading();
                if (res.success || res.status) {
                    uni.showToast({ title: this.$t('common.success') });
                    this.showTypeModal = false; // Close modal on success
                    this.fetchTypes();
                } else {
                    uni.showToast({ title: res.message || this.$t('common.fail'), icon: 'none' });
                }
            }).catch(() => {
                uni.hideLoading();
                uni.showToast({ title: this.$t('schedule.system_error'), icon: 'none' });
            });
        },


        // --- Helpers ---
        formatDateYMD(date) {
            const y = date.getFullYear();
            const m = (date.getMonth() + 1).toString().padStart(2, '0');
            const d = date.getDate().toString().padStart(2, '0');
            return `${y}-${m}-${d}`;
        },
        formatTime(dt) {
            if (!dt) return '';
            return dt.substring(11, 16);
        },
        formatTimeString(s) {
            if (!s) return '';
            if (s.length > 10) return s.substring(11, 16);
            return s.substring(0, 5);
        },
        formatDate(dt) {
            if (!dt) return '';
            return dt.substring(0, 10);
        },
        getShiftColor(cat) { return ''; }, // Simplified
        getCategoryText(c) {
            const map = {
                0: this.$t('schedule.type_normal'),
                1: this.$t('schedule.type_night'),
                2: this.$t('schedule.type_other')
            };
            return map[c] || this.$t('schedule.type_other');
        },
        getAuditStatusText(s) {
            const map = {
                0: this.$t('schedule.status_pending'),
                1: this.$t('schedule.status_approved'),
                2: this.$t('schedule.status_rejected')
            };
            return map[s] || this.$t('common.unknown');
        },
        getAuditStatusClass(s) {
            if (s === 1) return 'status-ok';
            if (s === 2) return 'status-err';
            return 'status-pend';
        }
    }
}
</script>

<style lang="scss" scoped>
.container {
    background: #f5f7fa;
    height: 100vh;
    display: flex;
    flex-direction: column;
}


.custom-nav {
    background-color: #fff;
    width: 100%;
    z-index: 90;
    box-shadow: 0 2rpx 8rpx rgba(0, 0, 0, 0.03);

    .nav-content {
        height: 88rpx;
        display: flex;
        align-items: center;
        justify-content: space-between;
        padding: 0 32rpx;

        .nav-left {
            width: 80rpx;
            display: flex;
            align-items: center;
        }

        .nav-title {
            font-size: 34rpx;
            font-weight: 600;
            color: #333;
        }

        .nav-right {
            width: 80rpx;
        }
    }
}

/* Custom Refresher */
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
    background-image: url("data:image/svg+xml;charset=utf-8,%3Csvg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 24 24' fill='%23409EFF'%3E%3Cpath d='M19 14h-1V8h-3V6h-2v2h-2V6H9v2H7v6H6c-1.1 0-2 .9-2 2v3c0 1.1.9 2 2 2h1c0 1.66 1.34 3 3 3s3-1.34 3-3h4c0 1.66 1.34 3 3 3s3-1.34 3-3h1c1.1 0 2-.9 2-2v-3c0-1.1-.9-2-2-2zM8 19c-.55 0-1-.45-1-1s.45-1 1-1 1 .45 1 1-.45 1-1 1zm10 0c-.55 0-1-.45-1-1s.45-1 1-1 1 .45 1 1-.45 1-1 1zM8 8h2v2H8V8zm0 4h2v2H8v-2zm4 4h-2v-2h2v2zm0-4h-2V8h2v4zm4 4h-2v-2h2v2zm0-4h-2V8h2v4z'/%3E%3Cpath d='M4 8h2v2H4zm0 4h2v2H4z' fill-opacity='.3'/%3E%3C/svg%3E");
    background-size: contain;
    background-repeat: no-repeat;
    transition: transform 0.5s linear;
}

.animating {
    animation: rotate 1s linear infinite;
}

.refresher-text {
    font-size: 24rpx;
    color: #999;
}

@keyframes rotate {
    0% {
        transform: rotate(0deg);
    }

    100% {
        transform: rotate(360deg);
    }
}


.empty-state {
    display: flex;
    flex-direction: column;
    align-items: center;
    justify-content: center;
    padding: 60rpx 0;
}

.empty-img {
    width: 200rpx;
    height: 200rpx;
    margin-bottom: 20rpx;
}


.row-top {
    display: flex;
    align-items: center;
    justify-content: space-between;
}

.row-between {
    display: flex;
    align-items: center;
    justify-content: space-between;
}

.empty-text {
    font-size: 28rpx;
    color: #999;
}



.icon-arrow-left {
    width: 40rpx;
    height: 40rpx;
    background: #333;
    -webkit-mask: url("data:image/svg+xml;charset=utf-8,%3Csvg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 24 24'%3E%3Cpath d='M20 11H7.83l5.59-5.59L12 4l-8 8 8 8 1.41-1.41L7.83 13H20v-2z'/%3E%3C/svg%3E") no-repeat center/contain;
    mask: url("data:image/svg+xml;charset=utf-8,%3Csvg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 24 24'%3E%3Cpath d='M20 11H7.83l5.59-5.59L12 4l-8 8 8 8 1.41-1.41L7.83 13H20v-2z'/%3E%3C/svg%3E") no-repeat center/contain;
}

.content {
    flex: 1;
    overflow-y: auto;
    padding: 0;
    /* Removed padding to keep scrollbar at edge */
}

.inner-content {
    padding: 0 32rpx 32rpx 32rpx;
}



.action-bar {
    margin-bottom: 24rpx;
    text-align: right;
}

.date-nav {
    display: flex;
    justify-content: space-between;
    align-items: center;
    margin-bottom: 24rpx;
}

.date-picker-view {
    background: #fff;
    padding: 16rpx 32rpx;
    border-radius: 30rpx;
    font-size: 30rpx;
    display: flex;
    align-items: center;
    gap: 10rpx;
}

.list-container {
    display: flex;
    flex-direction: column;
    gap: 20rpx;
}

.card {
    background: #fff;
    padding: 30rpx;
    border-radius: 16rpx;
    box-shadow: 0 2rpx 8rpx rgba(0, 0, 0, 0.02);
}

.card-header {
    display: flex;
    justify-content: space-between;
    align-items: center;
    margin-bottom: 16rpx;
    font-weight: bold;
    font-size: 30rpx;
}

.header-left {
    display: flex;
    align-items: center;
    gap: 16rpx;
    flex: 1;
    min-width: 0;
}

.header-right {
    display: flex;
    align-items: center;
    gap: 12rpx;
}

.action-menu {
    width: 64rpx;
    height: 64rpx;
    display: flex;
    align-items: center;
    justify-content: center;
    position: relative;
    cursor: pointer;
}

.menu-icon {
    font-size: 44rpx;
    line-height: 1;
    color: #999;
}

.action-dropdown {
    position: absolute;
    top: 100%;
    right: 0;
    background: #fff;
    border-radius: 12rpx;
    box-shadow: 0 8rpx 32rpx rgba(0, 0, 0, 0.12);
    z-index: 110;
    min-width: 220rpx;
    margin-top: 8rpx;
    border: 2rpx solid #f0f0f0;
    overflow: hidden;
}

.action-item {
    display: flex;
    align-items: center;
    padding: 24rpx 32rpx;
    gap: 16rpx;
    border-bottom: 2rpx solid #f5f5f5;
}

.action-item:last-child {
    border-bottom: none;
}

.action-item:active {
    background: #f5f7fa;
}

.action-icon {
    font-size: 32rpx;
}

.action-text {
    font-size: 28rpx;
    color: #333;
    white-space: nowrap;
}

.delete-item .action-text {
    color: #ff4d4f;
}

.card-body {
    font-size: 28rpx;
    color: #666;
    display: flex;
    flex-direction: column;
    gap: 8rpx;
}

.card-footer {
    margin-top: 20rpx;
    border-top: 1px solid #eee;
    padding-top: 20rpx;
    text-align: right;
}

.btn-primary {
    background: $sull-color-primary;
    color: #fff;
    border-radius: 40rpx;
    font-size: 28rpx;
    padding: 0 40rpx;
    height: 80rpx;
    line-height: 80rpx;
    border: none;
}

.btn-primary-small {
    background: $sull-color-primary;
    color: #fff;
    border-radius: 30rpx;
    font-size: 24rpx;
    padding: 0 30rpx;
    height: 60rpx;
    line-height: 60rpx;
}

/* Matrix View Styles */
.schedule-header {
    display: flex;
    justify-content: space-between;
    align-items: center;
    padding: 24rpx 0;
    margin-bottom: 20rpx;
}

.left-actions {
    flex: 1;
}

.btn-primary-icon {
    background: #009A97;
    color: #fff;
    border-radius: 40rpx;
    font-size: 28rpx;
    padding: 0 32rpx;
    height: 72rpx;
    line-height: 72rpx;
    border: none;
    display: inline-flex;
    align-items: center;
    gap: 8rpx;
}

.plus-icon {
    font-size: 32rpx;
    font-weight: bold;
}

.date-switcher {
    display: flex;
    align-items: center;
    gap: 20rpx;
    background: #fff;
    padding: 12rpx 24rpx;
    border-radius: 36rpx;
    box-shadow: 0 2rpx 8rpx rgba(0, 0, 0, 0.05);
}

.current-date {
    font-size: 30rpx;
    font-weight: bold;
    color: #333;
    min-width: 180rpx;
    text-align: center;
}

.schedule-matrix {
    background: #fff;
    border-radius: 16rpx;
    overflow: hidden;
    box-shadow: 0 2rpx 12rpx rgba(0, 0, 0, 0.03);
}

.matrix-row {
    display: flex;
    border-bottom: 1px solid #f0f0f0;
}

.matrix-row.header {
    background: #f9f9f9;
}

.matrix-cell {
    flex: 1;
    display: flex;
    align-items: center;
    justify-content: center;
    padding: 16rpx 4rpx;
    font-size: 24rpx;
    min-height: 80rpx;
}

.matrix-cell.name-col {
    width: 200rpx;
    min-width: 200rpx;
    max-width: 200rpx;
    flex: none;
    box-sizing: border-box;
    justify-content: flex-start;
    padding-left: 16rpx;
    border-right: 1px solid #f0f0f0;
    flex-direction: column;
    align-items: flex-start;
    overflow: hidden;
}

.matrix-row.header .name-col {
    align-items: center;
    justify-content: center;
    padding-left: 0;
}

.matrix-cell.name-col .user-name {
    flex: 1;
    min-width: 0;
    font-size: 24rpx;
    font-weight: 500;
    color: #333;
    white-space: nowrap;
    overflow: hidden;
    text-overflow: ellipsis;
    transition: font-size 0.2s;
}

.matrix-cell.name-col .user-name.small-name {
    font-size: 20rpx;
}

.matrix-cell.name-col .sub-text {
    font-size: 18rpx;
    color: #999;
}

.matrix-body .name-col {
    flex-direction: row;
    align-items: center;
}

.matrix-cell.date-col {
    flex-direction: column;
    gap: 4rpx;
}

.week-day {
    font-size: 22rpx;
    color: #666;
}

.day-date {
    font-size: 24rpx;
    font-weight: bold;
    color: #333;
}

.shift-tag {
    width: 48rpx;
    height: 48rpx;
    border-radius: 50%;
    display: flex;
    align-items: center;
    justify-content: center;
    font-size: 24rpx;
    color: #fff;
}

.shift-orange {
    background: #ff9c00;
}

.shift-blue {
    background: #1890ff;
}

.shift-purple {
    background: #722ed1;
}

.shift-leave {
    background: #99CC33;
}

.shift-default {
    background: #d9d9d9;
    color: #666;
}


/* Modal Overlay & Styles (Handover Style) */
.modal-overlay {
    position: fixed;
    top: 0;
    left: 0;
    width: 100%;
    height: 100%;
    background: rgba(0, 0, 0, 0.5);
    z-index: 100;
    display: flex;
    align-items: center;
    justify-content: center;
}

.modal-content {
    width: 85%;
    background: #fff;
    border-radius: 24rpx;
    display: flex;
    flex-direction: column;
    max-height: 80vh;
}

.modal-header {
    padding: 32rpx;
    border-bottom: 2rpx solid #eee;
    display: flex;
    justify-content: space-between;
    align-items: center;
}

.modal-title {
    font-size: 34rpx;
    font-weight: bold;
    color: #333;
}

.close-btn {
    padding: 8rpx;
    font-size: 36rpx;
    color: #999;
}

.modal-body {
    padding: 40rpx;
    overflow-y: auto;
    display: flex;
    flex-direction: column;
    gap: 32rpx;
}

.input-item {
    background: #f5f7fa;
    padding: 24rpx;
    border-radius: 12rpx;
    font-size: 28rpx;
    color: #333;
    display: flex;
    align-items: center;
}

.textarea {
    background: #f5f7fa;
    padding: 24rpx;
    border-radius: 12rpx;
    width: 100%;
    height: 200rpx;
    box-sizing: border-box;
    font-size: 28rpx;
    color: #333;
}

.picker-box {
    background: #f5f7fa;
    padding: 16rpx 24rpx;
    border-radius: 12rpx;
    font-size: 28rpx;
    color: #333;
    min-width: 120rpx;
    text-align: center;
}

.time-box {
    background: #f5f7fa;
    padding: 16rpx 24rpx;
    border-radius: 12rpx;
    font-size: 28rpx;
    color: #333;
    min-width: 100rpx;
    text-align: center;
}

.modal-footer {
    display: flex;
    flex-direction: row;
    /* reset to row */
    border-top: 2rpx solid #eee;
    padding: 0;
}

.modal-btn {
    flex: 1;
    border: none;
    background: #fff;
    border-radius: 0;
    padding: 28rpx 0;
    font-size: 30rpx;
    line-height: normal;
}

.modal-btn::after {
    border: none;
}

.modal-btn.cancel {
    color: #909399;
    border-right: 2rpx solid #eee;
    border-bottom-left-radius: 24rpx;
}

.modal-btn.confirm {
    color: #2979ff;
    font-weight: 500;
    border-bottom-right-radius: 24rpx;
}

.modal-btn:active {
    background: #f9f9f9;
}

.row-center {
    display: flex;
    align-items: center;
    gap: 20rpx;
}

/* Calendar Styles */
.calendar-header {
    display: flex;
    justify-content: center;
    padding: 24rpx 0;
}

.month-switcher {
    display: flex;
    align-items: center;
    gap: 32rpx;
    background: #fff;
    padding: 12rpx 32rpx;
    border-radius: 40rpx;
    box-shadow: 0 4rpx 12rpx rgba(0, 0, 0, 0.05);

    .current-month {
        font-size: 32rpx;
        font-weight: bold;
        color: #333;
        min-width: 160rpx;
        text-align: center;
    }
}

.calendar-grid {
    background: #fff;
    border-radius: 24rpx;
    padding: 24rpx;
    box-shadow: 0 4rpx 16rpx rgba(0, 0, 0, 0.03);
}

.calendar-weekdays {
    display: flex;
    margin-bottom: 24rpx;

    text {
        flex: 1;
        text-align: center;
        font-size: 24rpx;
        color: #999;
        font-weight: 500;
    }
}

.calendar-body {
    display: flex;
    flex-wrap: wrap;
}

.calendar-day {
    width: 14.28%;
    height: 120rpx;
    display: flex;
    flex-direction: column;
    align-items: center;
    justify-content: flex-start;
    padding: 8rpx 0;
    border-bottom: 1px solid #f8f8f8;

    .day-num {
        font-size: 28rpx;
        color: #333;
        margin-bottom: 8rpx;
    }

    &.other-month {
        opacity: 0.3;
    }
}

.calendar-shift-tag {
    font-size: 18rpx;
    padding: 2rpx 8rpx;
    border-radius: 6rpx;
    color: #fff;
    max-width: 90%;
    overflow: hidden;
    white-space: nowrap;
    text-overflow: ellipsis;
}

.status-tag {
    font-size: 20rpx;
    padding: 4rpx 16rpx;
    border-radius: 8rpx;
    font-weight: 500;
}

.status-ok {
    background: #f6ffed;
    color: #52c41a;
    border: 1px solid #b7eb8f;
}

.status-err {
    background: #fff1f0;
    color: #ff4d4f;
    border: 1px solid #ffa39e;
}

.status-pend {
    background: #fff7e6;
    color: #faad14;
    border: 1px solid #ffe58f;
}

.calendar-modal {
    width: 95% !important;
    max-height: 90vh;
}
</style>
