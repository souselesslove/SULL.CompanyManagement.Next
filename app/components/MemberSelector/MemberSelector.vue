<template>
    <view class="member-overlay" v-if="visible" @click="close" @touchmove.stop.prevent>
        <view class="member-modal" @click.stop>
            <view class="member-header">
                <text class="modal-title" v-if="title">{{ title }}</text>
                <view class="search-box">
                    <input class="member-search-input" v-model="searchKey" :placeholder="placeholder" />
                    <text class="clear-icon" v-if="searchKey" @click="searchKey = ''">×</text>
                </view>
            </view>
            <scroll-view class="member-list" scroll-y>
                <view class="member-item" v-for="(m, i) in filteredList" :key="i" @click="onSelect(m)">
                    <text class="member-name">{{ m.label }}</text>
                </view>
                <view v-if="filteredList.length === 0" class="empty-state-small">
                    <text>{{ loading ? '加载中...' : '无结果' }}</text>
                </view>
            </scroll-view>
        </view>
    </view>
</template>

<script>
import request from '@/utils/request.js';

export default {
    name: 'MemberSelector',
    props: {
        visible: {
            type: Boolean,
            default: false
        },
        title: {
            type: String,
            default: ''
        },
        placeholder: {
            type: String,
            default: '搜索成员'
        },
        // Optional: if the parent wants to provide the list
        members: {
            type: Array,
            default: null
        },
        // Optional: exclude specific user IDs
        excludeIds: {
            type: Array,
            default: () => []
        }
    },
    data() {
        return {
            searchKey: '',
            internalList: [],
            loading: false
        }
    },
    computed: {
        displayList() {
            const list = this.members !== null ? this.members : this.internalList;
            if (this.excludeIds.length > 0) {
                return list.filter(m => !this.excludeIds.includes(m.value));
            }
            return list;
        },
        filteredList() {
            if (!this.searchKey) return this.displayList;
            const key = this.searchKey.toLowerCase();
            return this.displayList.filter(m => m.label.toLowerCase().includes(key));
        }
    },
    watch: {
        visible(newVal) {
            if (newVal) {
                this.searchKey = '';
                if (this.members === null && this.internalList.length === 0) {
                    this.fetchData();
                }
            }
        }
    },
    methods: {
        fetchData() {
            this.loading = true;
            request.get('api/ssbh/OrganizationUser/DepartmentUser')
                .then(res => {
                    const list = res.rows || [];
                    this.internalList = list.map(m => {
                        const name = m.UserTrueName || m.userTrueName || m.UserName || m.userName || 'Unknown';
                        const dept = m.DepartmentName || m.departmentName;
                        return {
                            label: name + (dept ? ` (${dept})` : ''),
                            value: m.UserId || m.userId,
                            raw: m
                        };
                    });
                })
                .finally(() => {
                    this.loading = false;
                });
        },
        onSelect(member) {
            this.$emit('select', member);
            this.close();
        },
        close() {
            this.$emit('update:visible', false);
        }
    }
}
</script>

<style lang="scss" scoped>
@keyframes slideUp {
    from {
        transform: translateY(100%);
    }

    to {
        transform: translateY(0);
    }
}

.member-overlay {
    position: fixed;
    top: 0;
    left: 0;
    right: 0;
    bottom: 0;
    background: rgba(0, 0, 0, 0.5);
    z-index: 3000;
    display: flex;
    align-items: flex-end;
    justify-content: center;
    backdrop-filter: blur(4rpx);
}

.member-modal {
    width: 100%;
    background: #fff;
    border-radius: 40rpx 40rpx 0 0;
    display: flex;
    flex-direction: column;
    overflow: hidden;
    max-height: 70vh;
    padding-bottom: env(safe-area-inset-bottom);
    animation: slideUp 0.3s ease;
}

.member-header {
    padding: 32rpx;
    border-bottom: 1rpx solid #F5F7FA;

    .modal-title {
        display: block;
        font-size: 32rpx;
        font-weight: bold;
        color: #333;
        margin-bottom: 24rpx;
        text-align: center;
    }
}

.search-box {
    position: relative;
    display: flex;
    align-items: center;
}

.member-search-input {
    flex: 1;
    height: 80rpx;
    background: #F5F7FA;
    border-radius: 40rpx;
    padding: 0 80rpx 0 32rpx;
    font-size: 28rpx;
}

.clear-icon {
    position: absolute;
    right: 24rpx;
    color: #999;
    font-size: 32rpx;
    padding: 10rpx;
    z-index: 10;
}

.member-list {
    flex: 1;
    max-height: 60vh;
    overflow-y: auto;
}

.member-item {
    padding: 32rpx 40rpx;
    border-bottom: 1rpx solid #F5F7FA;
    font-size: 30rpx;
    color: #333;

    &:active {
        background-color: #F5F7FA;
    }
}

.empty-state-small {
    text-align: center;
    padding: 60rpx;
    color: #999;
    font-size: 26rpx;
}
</style>
