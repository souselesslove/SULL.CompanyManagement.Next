<template>
    <view class="container">
        <!-- Navigation Bar -->
        <view class="header-section" :style="{ paddingTop: statusBarHeight * 2 + 'rpx' }">
            <view class="header-content">
                <view class="nav-left">
                    <BackButton class="header-back-btn" @click="goBack" />
                </view>
                <text class="page-title">{{ itemName ? itemName + ' - ' + $t('itemManage.batch') :
                    $t('itemManage.batch_manage') }}</text>
                <view class="nav-right">
                    <view class="header-btn" @click="handleExport">
                        <text class="header-icon">📤</text>
                    </view>
                    <view class="header-btn" @click="handleImportExcel">
                        <text class="header-icon">📥</text>
                    </view>
                </view>
            </view>
        </view>

        <scroll-view class="main-scroll" scroll-y :enable-back-to-top="true" :refresher-enabled="true"
            :refresher-triggered="isRefreshing" @refresherrefresh="onRefresh" @refresherpulling="onPulling"
            @refresherrestore="onRestore" @scroll="onScroll" :scroll-top="scrollTop">

            <CustomRefresher slot="refresher" :isRefreshing="isRefreshing" :isPulling="isPulling" />

            <view class="content">
                <!-- Search Bar -->
                <view class="search-section">
                    <view class="search-box">
                        <text class="search-icon">🔍</text>
                        <input class="search-input" v-model="searchText" confirm-type="search" @confirm="onSearch"
                            :placeholder="$t('itemManage.search_batch_placeholder')" />
                        <view class="clear-icon" v-if="searchText" @click="clearSearch">
                            <text>✖</text>
                        </view>
                    </view>
                </view>

                <view class="list-content">
                    <view class="batch-card" v-for="(batch, index) in batchList" :key="index">
                        <view class="card-header">
                            <view class="name-info">
                                <text class="item-name">{{ batch.ItemName || '-' }}</text>
                                <text class="batch-no">{{ $t('itemManage.batch_no') }}: {{ batch.RegisterCode || '-'
                                }}</text>
                            </view>
                            <view class="status-tag" :class="getExpiryClass(batch.ExpirationDate)">
                                <text>{{ getExpiryText(batch.ExpirationDate) }}</text>
                            </view>
                        </view>

                        <view class="card-body">
                            <view class="info-row">
                                <text class="label">{{ $t('itemManage.factory') }}:</text>
                                <text class="value">{{ batch.Manufacturer || '-' }}</text>
                            </view>
                            <view class="info-row">
                                <text class="label">{{ $t('itemManage.expiry_to') }}:</text>
                                <text class="value">{{ formatDate(batch.ExpirationDate) }}</text>
                            </view>
                            <view class="info-row">
                                <text class="label">{{ $t('itemManage.rescue_car') }}:</text>
                                <text class="value">{{ batch.RescueCarName || '-' }}</text>
                            </view>
                            <view class="info-row">
                                <text class="label">{{ $t('itemManage.position') }}:</text>
                                <text class="value">{{ batch.PositionName || '-' }}</text>
                            </view>
                        </view>

                        <view class="card-footer">
                            <view class="stock-info">
                                <text class="label">{{ $t('itemManage.current_stock') }}:</text>
                                <text class="stock-val">{{ batch.Stock || 0 }}</text>
                                <text class="unit">{{ batch.Unit || '' }}</text>
                            </view>
                        </view>
                    </view>

                    <view v-if="batchList.length === 0 && !loading" class="empty-state">
                        <view class="empty-icon-bg">
                            <text class="empty-icon">📦</text>
                        </view>
                        <text class="empty-title">{{ $t('itemManage.no_batch_data') }}</text>
                    </view>
                </view>
            </view>
            <view class="bottom-spacer"></view>
            <FooterInfo />
        </scroll-view>

        <BackToTop :scroll-top="scrollTop" @click="goTop" />

        <!-- Excel Field Mapping Modal -->
        <view class="mapping-modal" v-if="showMappingModal" @touchmove.stop.prevent>
            <view class="modal-mask" @click="showMappingModal = false"></view>
            <view class="modal-container">
                <view class="modal-header">
                    <text class="modal-title">{{ $t('itemManage.field_mapping') }}</text>
                    <text class="close-btn" @click="showMappingModal = false">✕</text>
                </view>

                <scroll-view scroll-y class="modal-body">
                    <view class="mapping-list">
                        <view class="mapping-item" v-for="field in systemFields" :key="field.key">
                            <view class="field-info">
                                <text class="field-label">{{ field.label }}</text>
                                <text class="field-key">{{ field.key }}</text>
                            </view>
                            <picker mode="selector" :range="excelHeaders"
                                @change="handleMappingChange($event, field.key)">
                                <view class="excel-column-picker" :class="{ 'placeholder': !fieldMappings[field.key] }">
                                    <text>{{ fieldMappings[field.key] || $t('itemManage.excel_field') }}</text>
                                    <text class="picker-arrow">▾</text>
                                </view>
                            </picker>
                        </view>
                    </view>
                </scroll-view>

                <view class="modal-footer">
                    <view class="btn-cancel" @click="showMappingModal = false">
                        <text>{{ $t('common.cancel') }}</text>
                    </view>
                    <view class="btn-confirm" @click="confirmImport">
                        <text>{{ $t('common.confirm') }}</text>
                    </view>
                </view>
            </view>
        </view>
    </view>
</template>

<script>
import FooterInfo from '@/components/FooterInfo/FooterInfo.vue';
import BackButton from '@/components/BackButton/BackButton.vue';
import BackToTop from '@/components/BackToTop/BackToTop.vue';
import CustomRefresher from '@/components/CustomRefresher/CustomRefresher.vue';
import request from '@/utils/request.js';
import { storage } from '@/utils/storage.js';
import dayjs from 'dayjs';
import * as XLSX from 'xlsx';

export default {
    components: {
        FooterInfo,
        BackButton,
        BackToTop,
        CustomRefresher
    },
    data() {
        return {
            statusBarHeight: 20,
            searchText: '',
            batchList: [],
            page: 1,
            rows: 10,
            total: 0,
            loading: false,
            isRefreshing: false,
            isPulling: false,
            userOrgInfo: {},
            scrollTop: 0,
            oldScrollTop: 0,
            itemId: null,
            itemName: '',

            // Excel Mapping
            showMappingModal: false,
            excelHeaders: [],
            parsedJsonData: [],
            fieldMappings: {},
            systemFields: [
                { key: 'RegisterCode', label: '批号 / Batch No' },
                { key: 'ExpirationDate', label: '有效期 / Expiry Date' },
                { key: 'Stock', label: '库存 / Stock' },
                { key: 'Manufacturer', label: '生产厂家 / Manufacturer' },
                { key: 'PositionName', label: '存放位置 / Position' }
            ]
        };
    },
    onLoad(options) {
        // Load saved mappings
        const savedMappings = storage.get('excelFieldMappings') || {};
        this.fieldMappings = savedMappings;
        // Handle status bar
        const sysInfo = uni.getSystemInfoSync();
        this.statusBarHeight = sysInfo.statusBarHeight;

        if (options && options.itemId) {
            this.itemId = options.itemId;
        }
        if (options && options.itemName) {
            this.itemName = decodeURIComponent(options.itemName);
        }

        const uInfo = storage.get('userOrgInfo');
        if (uInfo) {
            this.userOrgInfo = uInfo;
        }

        this.fetchBatches();
    },
    methods: {
        formatDate(dateStr) {
            if (!dateStr) return '-';
            return dateStr.substring(0, 10);
        },
        getDaysUntil(dateStr) {
            if (!dateStr) return 9999;
            const now = new Date();
            const target = dayjs(dateStr).toDate();
            const diff = target.getTime() - now.getTime();
            return Math.ceil(diff / (1000 * 3600 * 24));
        },
        getExpiryClass(dateStr) {
            const days = this.getDaysUntil(dateStr);
            if (days < 0) return 'tag-expired';
            if (days < 30) return 'tag-urgent';
            if (days <= 90) return 'tag-warning';
            return 'tag-normal';
        },
        getExpiryText(dateStr) {
            const days = this.getDaysUntil(dateStr);
            if (days < 0) return this.$t('itemManage.expired');
            if (days < 30) return this.$t('itemManage.days_left', { days: days });
            if (days <= 90) return this.$t('itemManage.days_left', { days: days });
            return this.$t('itemManage.normal');
        },
        goBack() {
            uni.navigateBack();
        },
        onSearch() {
            this.page = 1;
            this.batchList = [];
            this.fetchBatches();
        },
        clearSearch() {
            this.searchText = '';
            this.onSearch();
        },
        onPulling() {
            this.isPulling = true;
        },
        onRestore() {
            this.isRefreshing = false;
        },
        onRefresh() {
            this.isRefreshing = true;
            this.page = 1;
            this.fetchBatches().then(() => {
                setTimeout(() => {
                    this.isRefreshing = false;
                }, 800);
            });
        },
        fetchBatches() {
            if (this.loading) return Promise.resolve();
            this.loading = true;

            const wheres = [];

            if (this.userOrgInfo?.Organization?.Id) {
                wheres.push({ name: 'OrganizationId', value: this.userOrgInfo.Organization.Id });
            }
            if (this.userOrgInfo?.Department?.Id) {
                wheres.push({ name: 'DepartmentId', value: this.userOrgInfo.Department.Id });
            }

            if (this.searchText) {
                // Search in RegisterCode or ItemName (assuming support or just search code)
                wheres.push({ name: 'RegisterCode', value: this.searchText, displayType: 'like' });
            }

            if (this.itemId) {
                wheres.push({ name: 'ItemId', value: this.itemId });
            }

            return request.post('api/ssbh/ItemDetail/GetPageData', {
                page: this.page,
                rows: this.rows,
                sort: 'Id',
                order: 'desc',
                wheres: JSON.stringify(wheres)
            }).then(res => {
                const rows = res.rows || [];
                if (this.page === 1) {
                    this.batchList = rows;
                } else {
                    this.batchList = [...this.batchList, ...rows];
                }
                this.total = res.total;
                this.loading = false;
            }).catch(e => {
                console.error("Fetch Batches Failed", e);
                this.loading = false;
            });
        },
        onScroll(e) {
            this.scrollTop = e.detail.scrollTop;
            this.oldScrollTop = e.detail.scrollTop;
        },
        goTop() {
            this.scrollTop = this.oldScrollTop;
            this.$nextTick(() => {
                this.scrollTop = 0;
            });
        },
        async handleExport() {
            if (this.batchList.length === 0) {
                uni.showToast({ title: this.$t('itemManage.no_data_export'), icon: 'none' });
                return;
            }

            // #ifdef APP-PLUS
            if (uni.getSystemInfoSync().platform === 'android') {
                const permission = 'android.permission.WRITE_EXTERNAL_STORAGE';
                const hasPermission = await new Promise((resolve) => {
                    plus.android.requestPermissions([permission], (res) => {
                        if (res.granted.length > 0) resolve(true);
                        else resolve(false);
                    }, (err) => resolve(false));
                });

                if (!hasPermission) {
                    uni.showModal({
                        title: this.$t('itemManage.permission_request'),
                        content: this.$t('itemManage.permission_storage_write'),
                        confirmText: this.$t('itemManage.go_to_settings'),
                        success: (res) => {
                            if (res.confirm) {
                                plus.runtime.openAppSettings();
                            }
                        }
                    });
                    return;
                }
            }
            // #endif

            uni.showLoading({ title: this.$t('itemManage.exporting'), mask: true });

            try {
                const headers = this.systemFields.map(f => f.label.split(' / ')[0]);
                const keys = this.systemFields.map(f => f.key);
                // Prepend ItemName for clarity
                headers.unshift(this.$t('itemManage.name'));
                keys.unshift('ItemName');

                const data = this.batchList.map(row => keys.map(k => row[k]));
                const wb = XLSX.utils.book_new();
                const ws = XLSX.utils.aoa_to_sheet([headers, ...data]);
                XLSX.utils.book_append_sheet(wb, ws, "Sheet1");

                const fileName = `批次列表_${this.itemName || ''}_${dayjs().format('YYYYMMDD')}.xlsx`;

                // #ifdef H5
                XLSX.writeFile(wb, fileName);
                uni.hideLoading();
                // #endif

                // #ifdef MP-WEIXIN
                const wbout_wx = XLSX.write(wb, { bookType: 'xlsx', type: 'array' });
                const fs = wx.getFileSystemManager();
                const filePath_wx = `${wx.env.USER_DATA_PATH}/${fileName}`;
                fs.writeFile({
                    filePath: filePath_wx,
                    data: wbout_wx,
                    encoding: 'binary',
                    success: () => {
                        uni.hideLoading();
                        uni.openDocument({ filePath: filePath_wx, showMenu: true });
                    },
                    fail: () => {
                        uni.hideLoading();
                        uni.showToast({ title: this.$t('common.fail'), icon: 'none' });
                    }
                });
                // #endif

                // #ifdef APP-PLUS
                const wbout_app = XLSX.write(wb, { bookType: 'xlsx', type: 'base64' });
                if (uni.getSystemInfoSync().platform === 'android') {
                    try {
                        const File = plus.android.importClass("java.io.File");
                        const FileOutputStream = plus.android.importClass("java.io.FileOutputStream");
                        const Base64 = plus.android.importClass("android.util.Base64");
                        const Environment = plus.android.importClass("android.os.Environment");

                        const publicDir = Environment.getExternalStoragePublicDirectory(Environment.DIRECTORY_DOWNLOADS);
                        let downloadDir = "/storage/emulated/0/Download";
                        if (publicDir) {
                            downloadDir = publicDir.getAbsolutePath();
                        }
                        const filePath = downloadDir + "/" + fileName;

                        const file = new File(filePath);
                        if (file.exists()) {
                            file.delete();
                        }
                        const stream = new FileOutputStream(file);
                        const bytes = Base64.decode(wbout_app, 0);
                        stream.write(bytes);
                        stream.flush();
                        stream.close();

                        // Verification check
                        if (!file.exists() || file.length() === 0) {
                            throw new Error("文件物理创建失败，请检查存储权限");
                        }

                        // Notify system to scan the file so it appears in file managers (crucial for modern Android)
                        const context = plus.android.runtimeMainActivity().getContext();
                        const MediaScannerConnection = plus.android.importClass("android.media.MediaScannerConnection");
                        MediaScannerConnection.scanFile(context, [filePath], null, null);

                        uni.hideLoading();
                        uni.showModal({
                            title: '导出成功',
                            content: `文件已保存至：\n${filePath}\n\n是否立即打开查看？`,
                            confirmText: '打开',
                            cancelText: '取消',
                            success: (modalRes) => {
                                if (modalRes.confirm) {
                                    plus.runtime.openFile(filePath, {}, (err) => {
                                        uni.showToast({ title: this.$t('itemManage.access_file_fail'), icon: 'none' });
                                    });
                                }
                            }
                        });
                    } catch (e) {
                        uni.hideLoading();
                        uni.showToast({ title: this.$t('itemManage.export_fail'), icon: 'none' });
                    }
                } else {
                    // iOS fallback
                    const path = '_doc/' + fileName;
                    const fsManager = uni.getFileSystemManager();
                    fsManager.writeFile({
                        filePath: plus.io.convertLocalFileSystemURL(path),
                        data: wbout_app,
                        encoding: 'base64',
                        success: () => {
                            uni.hideLoading();
                            uni.showModal({ title: this.$t('itemManage.export_success'), content: this.$t('itemManage.export_to_doc'), showCancel: false });
                        },
                        fail: () => { uni.hideLoading(); uni.showToast({ title: this.$t('common.fail'), icon: 'none' }); }
                    });
                }
                // #endif
            } catch (e) {
                uni.hideLoading();
                uni.showToast({ title: this.$t('itemManage.export_error'), icon: 'none' });
            }
        },
        async handleImportExcel() {
            // #ifdef APP-PLUS
            if (uni.getSystemInfoSync().platform === 'android') {
                const permission = 'android.permission.READ_EXTERNAL_STORAGE';
                const hasPermission = await new Promise((resolve) => {
                    plus.android.requestPermissions([permission], (res) => {
                        resolve(res.granted.length > 0);
                    }, () => resolve(false));
                });
                if (!hasPermission) {
                    uni.showModal({
                        title: this.$t('itemManage.permission_request'),
                        content: this.$t('itemManage.permission_storage_read'),
                        confirmText: this.$t('common.confirm'),
                        success: (res) => { if (res.confirm) plus.runtime.openAppSettings(); }
                    });
                    return;
                }
            }
            // #endif

            const options = {
                count: 1,
                extension: ['.xlsx', '.xls'],
                success: (res) => {
                    this.processImportFile(res.tempFiles[0]);
                },
                fail: (err) => {
                    if (err && err.errMsg && err.errMsg.indexOf('cancel') === -1) {
                        uni.showToast({ title: this.$t('itemManage.select_cancel_fail'), icon: 'none' });
                    }
                }
            };

            // #ifdef MP-WEIXIN
            uni.chooseMessageFile({ type: 'file', ...options });
            // #endif

            // #ifdef APP-PLUS
            if (typeof uni.chooseFile === 'function') {
                uni.chooseFile(options);
            } else if (typeof uni.chooseMessageFile === 'function') {
                uni.chooseMessageFile({ type: 'file', ...options });
            } else if (uni.getSystemInfoSync().platform === 'android') {
                this.androidFilePicker(options.success);
            } else {
                uni.showToast({ title: this.$t('itemManage.env_not_support_file'), icon: 'none' });
            }
            // #endif

            // #ifdef H5
            uni.chooseFile(options);
            // #endif
        },
        androidFilePicker(successCallback) {
            const Intent = plus.android.importClass("android.content.Intent");
            const intent = new Intent(Intent.ACTION_GET_CONTENT);
            intent.setType("*/*");
            intent.addCategory(Intent.CATEGORY_OPENABLE);
            const main = plus.android.runtimeMainActivity();
            main.onActivityResult = (requestCode, resultCode, data) => {
                if (resultCode === -1 && data) {
                    try {
                        const uri = data.getData();
                        const path = uri ? uri.toString() : '';
                        successCallback({
                            tempFiles: [{
                                path: path,
                                name: 'Batch_Import_' + Date.now() + '.xlsx'
                            }]
                        });
                    } catch (e) {
                        uni.hideLoading();
                        uni.showToast({ title: this.$t('itemManage.select_file_fail'), icon: 'none' });
                    }
                } else {
                    uni.hideLoading();
                }
            };
            main.startActivityForResult(intent, 1000);
        },
        processImportFile(file) {
            const path = file.path;
            uni.showLoading({ title: this.$t('itemManage.extracting_data') });

            // #ifdef H5
            const reader = new FileReader();
            reader.onload = (e) => {
                uni.hideLoading();
                this.parseRawExcel(new Uint8Array(e.target.result));
            };
            reader.readAsArrayBuffer(file);
            // #endif

            // #ifdef APP-PLUS || MP-WEIXIN
            const timeout = setTimeout(() => {
                uni.hideLoading();
                uni.showToast({ title: this.$t('itemManage.read_timeout'), icon: 'none' });
            }, 15000);

            // Native Android ContentResolver reader
            if (uni.getSystemInfoSync().platform === 'android') {
                try {
                    const main = plus.android.runtimeMainActivity();
                    const resolver = plus.android.invoke(main, "getContentResolver");
                    const Uri = plus.android.importClass("android.net.Uri");
                    const uri = Uri.parse(path);
                    const inputStream = plus.android.invoke(resolver, "openInputStream", uri);

                    if (inputStream) {
                        const available = plus.android.invoke(inputStream, "available");
                        if (available > 0) {
                            const buf = plus.android.newObject("byte[]", available);
                            plus.android.invoke(inputStream, "read", buf);
                            plus.android.invoke(inputStream, "close");

                            const Base64 = plus.android.importClass("android.util.Base64");
                            const base64Data = Base64.encodeToString(buf, Base64.NO_WRAP);

                            clearTimeout(timeout);
                            uni.hideLoading();
                            this.parseRawExcel(base64Data, 'base64');
                            return;
                        }
                    }
                } catch (e) {
                    console.error('Native read fail', e);
                }
            }

            const readData = (filePath) => {
                const fs = uni.getFileSystemManager();
                fs.readFile({
                    filePath: filePath,
                    success: (res) => {
                        clearTimeout(timeout);
                        uni.hideLoading();
                        this.parseRawExcel(new Uint8Array(res.data));
                    },
                    fail: (err) => {
                        // Fallback to native IO for App-Plus (Fix for some Android URIs)
                        // #ifdef APP-PLUS
                        plus.io.resolveLocalFileSystemURL(filePath, (entry) => {
                            entry.file((fileObj) => {
                                const reader = new plus.io.FileReader();
                                reader.onloadend = (e) => {
                                    clearTimeout(timeout);
                                    uni.hideLoading();
                                    this.parseRawExcel(new Uint8Array(e.target.result));
                                };
                                reader.onerror = () => {
                                    clearTimeout(timeout);
                                    uni.hideLoading();
                                    uni.showToast({ title: this.$t('itemManage.read_content_fail'), icon: 'none' });
                                };
                                reader.readAsArrayBuffer(fileObj);
                            }, () => {
                                clearTimeout(timeout);
                                uni.hideLoading();
                                uni.showToast({ title: this.$t('itemManage.access_file_fail'), icon: 'none' });
                            });
                        }, (e) => {
                            clearTimeout(timeout);
                            uni.hideLoading();
                            uni.showToast({ title: this.$t('itemManage.path_parse_fail'), icon: 'none' });
                        });
                        // #endif

                        // #ifndef APP-PLUS
                        clearTimeout(timeout);
                        uni.hideLoading();
                        uni.showToast({ title: this.$t('itemManage.file_read_fail'), icon: 'none' });
                        // #endif
                    }
                });
            };

            readData(path);
            // #endif
        },
        parseRawExcel(data, type = 'array') {
            try {
                const workbook = XLSX.read(data, { type: type });
                this.processWorkbook(workbook);
            } catch (err) {
                uni.showToast({ title: this.$t('itemManage.excel_parse_fail'), icon: 'none' });
            }
        },
        processWorkbook(workbook) {
            const sheetNames = workbook.SheetNames;
            const firstSheet = workbook.Sheets[sheetNames[0]];

            // Get headers (first row)
            const range = XLSX.utils.decode_range(firstSheet['!ref']);
            const headers = [];
            for (let C = range.s.c; C <= range.e.c; ++C) {
                const address = XLSX.utils.encode_col(C) + '1';
                if (firstSheet[address]) {
                    headers.push(firstSheet[address].v);
                }
            }

            this.excelHeaders = headers;
            this.parsedJsonData = XLSX.utils.sheet_to_json(firstSheet);

            if (this.parsedJsonData.length > 0) {
                // Auto-match based on previous mappings, but also handle deletions
                this.validateMappings();
                this.showMappingModal = true;
            } else {
                uni.showToast({ title: this.$t('common.no_data'), icon: 'none' });
            }
        },
        validateMappings() {
            // Remove mappings where the Excel column no longer exists in current file
            const newMappings = { ...this.fieldMappings };
            let changed = false;
            Object.keys(newMappings).forEach(key => {
                if (newMappings[key] && !this.excelHeaders.includes(newMappings[key])) {
                    delete newMappings[key];
                    changed = true;
                }
            });
            if (changed) {
                this.fieldMappings = newMappings;
                this.saveStorageMappings();
            }
        },
        handleMappingChange(e, systemKey) {
            const index = e.detail.value;
            const selectedHeader = this.excelHeaders[index];

            // If selected header is empty (though picker shouldn't allow unless we added empty option)
            this.fieldMappings[systemKey] = selectedHeader;
            this.saveStorageMappings();

            // Trigger reactive update for Vue 2 if needed (uni-app VUE2)
            this.fieldMappings = JSON.parse(JSON.stringify(this.fieldMappings));
        },
        saveStorageMappings() {
            storage.set('excelFieldMappings', this.fieldMappings);
        },
        confirmImport() {
            // Check if at least one mapping is done
            const activeMappings = Object.values(this.fieldMappings).filter(v => !!v);
            if (activeMappings.length === 0) {
                uni.showToast({ title: this.$t('itemManage.please_pair_field'), icon: 'none' });
                return;
            }

            uni.showLoading({ title: this.$t('itemManage.uploading') });

            // Map the data
            const mappedData = this.parsedJsonData.map(row => {
                const item = {};
                Object.keys(this.fieldMappings).forEach(sysKey => {
                    const excelCol = this.fieldMappings[sysKey];
                    if (excelCol && row[excelCol] !== undefined) {
                        item[sysKey] = row[excelCol];
                    }
                });
                return item;
            });

            console.log('Final Mapped Data for API:', mappedData);

            // TODO: Call backend API to batch add/update
            setTimeout(() => {
                uni.hideLoading();
                this.showMappingModal = false;
                uni.showToast({ title: this.$t('itemManage.import_count', { count: mappedData.length }), icon: 'success' });
                this.onRefresh();
            }, 1000);
        }
    }
}
</script>

<style lang="scss" scoped>
.container {
    background-color: $sull-color-background;
    height: 100vh;
    display: flex;
    flex-direction: column;
}

.header-section {
    background-color: $sull-color-white;
    position: fixed;
    top: 0;
    left: 0;
    width: 100%;
    z-index: 999;
    box-shadow: 0 1px 4px rgba(0, 0, 0, 0.03);

    .header-content {
        height: 44px;
        display: flex;
        align-items: center;
        justify-content: space-between;
        padding: 0 16px;

        .nav-left,
        .nav-right {
            width: 40px;
        }

        .page-title {
            font-size: $font-size-h6;
            font-weight: $font-weight-semibold;
            color: $sull-text-main;
        }

        .header-btn {
            width: 72rpx;
            height: 72rpx;
            background: #f0f7ff;
            border-radius: 12rpx;
            display: flex;
            align-items: center;
            justify-content: center;
            margin-left: 16rpx;

            .header-icon {
                font-size: 36rpx;
            }

            &:active {
                opacity: 0.7;
            }
        }
    }
}

.main-scroll {
    flex: 1;
    height: 0;
}

.content {
    padding: 100px 32rpx 32rpx; // Account for fixed header
}

.search-section {
    margin-bottom: 24rpx;
}

.search-box {
    background: #fff;
    border-radius: 999px;
    height: 36px;
    display: flex;
    align-items: center;
    padding: 0 12px;
    box-shadow: 0 2rpx 8rpx rgba(0, 0, 0, 0.02);
}

.search-icon {
    font-size: 14px;
    margin-right: 8px;
    color: $sull-text-secondary;
}

.search-input {
    flex: 1;
    font-size: 28rpx;
}

.list-content {
    display: flex;
    flex-direction: column;
    gap: 24rpx;
}

.batch-card {
    background: #fff;
    border-radius: 16rpx;
    padding: 24rpx;
    box-shadow: 0 4rpx 16rpx rgba(0, 0, 0, 0.03);
}

.card-header {
    display: flex;
    justify-content: space-between;
    align-items: flex-start;
    margin-bottom: 16rpx;
    border-bottom: 1rpx solid #f5f5f5;
    padding-bottom: 16rpx;
}

.name-info {
    flex: 1;
}

.item-name {
    font-size: 30rpx;
    font-weight: bold;
    color: #333;
    display: block;
    margin-bottom: 4rpx;
}

.batch-no {
    font-size: 24rpx;
    color: #666;
}

.status-tag {
    padding: 4rpx 16rpx;
    border-radius: 6rpx;
    font-size: 22rpx;
    font-weight: 500;
}

.tag-expired {
    background: #333;
    color: #fff;
}

.tag-urgent {
    background: #ff4d4f;
    color: #fff;
}

.tag-warning {
    background: #faad14;
    color: #fff;
}

.tag-normal {
    background: #f0f9eb;
    color: #67c23a;
}

.card-body {
    padding: 12rpx 0;
    display: flex;
    flex-direction: column;
    gap: 8rpx;
}

.info-row {
    display: flex;
    font-size: 26rpx;
}

.label {
    color: #999;
    width: 140rpx;
}

.value {
    color: #444;
    flex: 1;
}

.card-footer {
    border-top: 1rpx solid #f5f5f5;
    padding-top: 16rpx;
    margin-top: 8rpx;
    display: flex;
    justify-content: flex-end;
}

.stock-info {
    display: flex;
    align-items: baseline;
    gap: 8rpx;

    .label {
        font-size: 24rpx;
        width: auto;
    }

    .stock-val {
        font-size: 32rpx;
        font-weight: bold;
        color: $sull-color-primary;
    }

    .unit {
        font-size: 24rpx;
        color: #999;
    }
}

.empty-state {
    padding: 120rpx 0;
    display: flex;
    flex-direction: column;
    align-items: center;
}

.empty-icon-bg {
    width: 200rpx;
    height: 200rpx;
    background: #fdfdfd;
    border-radius: 50%;
    display: flex;
    align-items: center;
    justify-content: center;
    margin-bottom: 24rpx;
}

.empty-icon {
    font-size: 100rpx;
}

.empty-title {
    font-size: 28rpx;
    color: #999;
}

.bottom-spacer {
    height: 48rpx;
}



.mapping-modal {
    position: fixed;
    top: 0;
    left: 0;
    right: 0;
    bottom: 0;
    z-index: 1001;
    display: flex;
    align-items: flex-end;

    .modal-mask {
        position: absolute;
        top: 0;
        left: 0;
        right: 0;
        bottom: 0;
        background: rgba(0, 0, 0, 0.5);
    }

    .modal-container {
        position: relative;
        width: 100%;
        background: #fff;
        border-radius: 32rpx 32rpx 0 0;
        padding-bottom: env(safe-area-inset-bottom);
        animation: slideUp 0.3s ease-out;
    }
}

.modal-header {
    padding: 32rpx;
    display: flex;
    justify-content: space-between;
    align-items: center;
    border-bottom: 1rpx solid #f5f5f5;

    .modal-title {
        font-size: 32rpx;
        font-weight: bold;
        color: #333;
    }

    .close-btn {
        font-size: 40rpx;
        color: #999;
        padding: 10rpx;
    }
}

.modal-body {
    max-height: 60vh;
}

.mapping-list {
    padding: 24rpx;
}

.mapping-item {
    display: flex;
    justify-content: space-between;
    align-items: center;
    padding: 32rpx 0;
    border-bottom: 1rpx solid #f9f9f9;

    .field-info {
        display: flex;
        flex-direction: column;
        gap: 4rpx;

        .field-label {
            font-size: 28rpx;
            color: #333;
            font-weight: 500;
        }

        .field-key {
            font-size: 22rpx;
            color: #999;
        }
    }

    .excel-column-picker {
        min-width: 240rpx;
        height: 72rpx;
        background: #f7f8fa;
        border-radius: 12rpx;
        display: flex;
        align-items: center;
        justify-content: space-between;
        padding: 0 20rpx;
        font-size: 26rpx;
        color: $sull-color-primary;

        &.placeholder {
            color: #999;
        }

        .picker-arrow {
            font-size: 24rpx;
            margin-left: 12rpx;
        }
    }
}

.modal-footer {
    padding: 32rpx;
    display: flex;
    gap: 24rpx;

    .btn-cancel,
    .btn-confirm {
        flex: 1;
        height: 88rpx;
        border-radius: 44rpx;
        display: flex;
        align-items: center;
        justify-content: center;
        font-size: 30rpx;
        font-weight: 600;
    }

    .btn-cancel {
        background: #f5f5f5;
        color: #666;
    }

    .btn-confirm {
        background: $sull-color-primary;
        color: #fff;
    }
}

@keyframes slideUp {
    from {
        transform: translateY(100%);
    }

    to {
        transform: translateY(0);
    }
}
</style>
