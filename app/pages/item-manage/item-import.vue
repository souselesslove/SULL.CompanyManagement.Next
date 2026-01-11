<template>
    <view class="container">
        <!-- Navigation Bar -->
        <view class="custom-nav" :style="{ paddingTop: statusBarHeight * 2 + 'rpx' }">
            <view class="nav-content">
                <view class="nav-left">
                    <BackButton class="nav-back-btn" @click="goBack" />
                </view>
                <text class="nav-title">{{ $t('itemManage.import_title') }}</text>
                <view class="nav-right"></view>
            </view>
        </view>

        <scroll-view class="import-content" scroll-y :style="{ marginTop: (statusBarHeight * 2 + 88) + 'rpx' }">

            <!-- Step 1: File Selection -->
            <view class="step-card">
                <view class="step-header">
                    <text class="step-num">1</text>
                    <text class="step-title">{{ $t('itemManage.select_file') }}</text>
                </view>

                <view class="file-area" @click="chooseFile">
                    <view v-if="!fileName" class="upload-placeholder">
                        <text class="icon">📄</text>
                        <text class="text">{{ $t('itemManage.select_file') }}</text>
                    </view>
                    <view v-else class="file-info">
                        <text class="name" @longpress="showFullName">{{ truncatedFileName }}</text>
                        <text class="change-btn">{{ $t('organization.edit') }}</text>
                    </view>
                </view>

                <!-- Sheet Selection -->
                <view class="sheet-select" v-if="sheetNames.length > 0">
                    <text class="label">{{ $t('itemManage.select_sheet') }}</text>
                    <picker :range="sheetNames" @change="onSheetChange" :value="cleanSheetIndex">
                        <view class="picker-view">
                            <text>{{ selectedSheetName || $t('itemManage.select_sheet') }}</text>
                            <text class="arrow">▼</text>
                        </view>
                    </picker>
                </view>
            </view>

            <!-- Step 2: Tabs -->
            <view class="tab-section" v-if="excelHeaders.length > 0">
                <view class="tab-header">
                    <view class="tab-item" :class="{ active: currentTab === 0 }" @click="currentTab = 0">{{
                        $t('itemManage.field_mapping') }}</view>
                    <view class="tab-item" :class="{ active: currentTab === 1 }" @click="currentTab = 1">
                        {{ $t('itemManage.upload_results') }}
                        <text class="badge" v-if="importResults.length > 0">{{ importResults.length }}</text>
                    </view>
                </view>
            </view>

            <!-- Step 3: Content -->
            <view class="step-card" v-if="excelHeaders.length > 0">
                <!-- Mapping Content -->
                <view class="mapping-list" v-show="currentTab === 0">
                    <view class="mapping-row header">
                        <text class="col-sys">{{ $t('itemManage.sys_field') }}</text>
                        <text class="col-excel">{{ $t('itemManage.excel_field') }}</text>
                        <text class="col-preview">预览 (前3行)</text>
                    </view>

                    <view class="mapping-row" v-for="(field, key) in sysFields" :key="key">
                        <view class="col-sys">
                            <text class="field-label">{{ field.label }}</text>
                            <text class="required-mark" v-if="field.required">*</text>
                        </view>
                        <view class="col-excel">
                            <picker :range="displayExcelHeaders" @change="(e) => onMappingChange(e, key)"
                                :value="getMappingIndex(key)">
                                <view class="picker-cell" :class="{ empty: !mapping[key] }">
                                    {{ mapping[key] || $t('itemManage.please_select') }}
                                </view>
                            </picker>
                        </view>
                        <view class="col-preview">
                            <text class="preview-text">{{ getPreviewData(key) }}</text>
                        </view>
                    </view>
                </view>

                <!-- Result Content -->
                <view class="result-list" v-show="currentTab === 1">
                    <view class="result-empty" v-if="importResults.length === 0">
                        <text>暂无异常信息</text>
                    </view>
                    <view class="result-item" v-for="(err, idx) in importResults" :key="idx">
                        <text class="error-msg">{{ err }}</text>
                    </view>
                </view>
            </view>

            <!-- Step 3: Action -->
            <view class="action-area" v-if="excelHeaders.length > 0 && currentTab === 0">
                <button class="submit-btn" :loading="uploading" :disabled="!canImport || uploading"
                    @click="startImport">
                    {{ uploading ? '正在导入...' : $t('itemManage.start_import') }}
                </button>
            </view>

        </scroll-view>
    </view>
</template>

<script>
import BackButton from '@/components/BackButton/BackButton.vue';
import request from '@/utils/request.js';
import { storage } from '@/utils/storage.js';
import * as XLSX from 'xlsx';

const CACHE_KEY = 'ITEM_IMPORT_CACHE';
const PERSIST_MAPPING_KEY = 'ITEM_IMPORT_FIELD_MAPPINGS';

export default {
    components: { BackButton },
    data() {
        return {
            statusBarHeight: 20,
            fileName: '',
            sheetNames: [],
            selectedSheetName: '',
            workbook: null,
            excelHeaders: [], // Array of strings
            excelData: [], // Array of objects


            mapping: {}, // key (sysField) -> value (excelHeaderName)
            uploading: false,
            progress: 0,
            totalRows: 0,
            invalidRows: [],
            importResults: [],
            currentTab: 0,
            userInfo: {},
            userOrgInfo: {}
        };
    },
    computed: {
        sysFields() {
            return {
                Name: { label: this.$t('itemManage.name'), required: false },
                FullName: { label: this.$t('itemManage.full_name'), required: false },
                InterfaceCode: { label: this.$t('itemManage.code'), required: false },
                Category: { label: this.$t('itemManage.category'), required: false },
                Specification: { label: this.$t('itemManage.spec'), required: false },
                Unit: { label: this.$t('itemManage.unit'), required: false },
                Form: { label: this.$t('itemManage.form'), required: false },
                Price: { label: this.$t('itemManage.price') || 'Price', required: false },
                RegisterCode: { label: this.$t('itemManage.register_code'), required: false },
                Factory: { label: this.$t('itemManage.factory'), required: false },
                Cardinality: { label: this.$t('itemManage.cardinality'), required: false },
                Barcode: { label: this.$t('itemManage.barcode'), required: false },
                OrderBy: { label: this.$t('itemManage.sort'), required: false },
                SearchCode: { label: this.$t('itemManage.search_code'), required: false },
                Remark: { label: this.$t('itemManage.remark'), required: false },
                Tag: { label: 'Tag', required: false },
                Layer: { label: 'Layer', required: false },
                LayerNO: { label: 'Layer NO', required: false },
                ColumnNO: { label: 'Column NO', required: false }
            };
        },
        displayExcelHeaders() {
            return [this.$t('itemManage.please_select'), ...this.excelHeaders];
        },
        cleanSheetIndex() {
            return this.sheetNames.indexOf(this.selectedSheetName);
        },
        canImport() {
            return this.excelData.length > 0
                && (this.mapping.Name || this.mapping.FullName);
        },
        truncatedFileName() {
            if (!this.fileName) return '';
            if (this.fileName.length > 20) {
                return this.fileName.substring(0, 18) + '...';
            }
            return this.fileName;
        }
    },
    onLoad() {
        const sysInfo = uni.getSystemInfoSync();
        this.statusBarHeight = sysInfo.statusBarHeight || 20;
        const uInfo = storage.get('userOrgInfo');
        if (uInfo) {
            this.userOrgInfo = uInfo;
        }
        if (uInfo && uInfo.User) {
            this.userInfo = uInfo.User;
        }
        this.loadCache();
    },
    methods: {
        loadCache() {
            try {
                const cache = storage.get(CACHE_KEY);
                if (cache) {
                    this.fileName = cache.fileName || '';
                    this.sheetNames = cache.sheetNames || [];
                    this.selectedSheetName = cache.selectedSheetName || '';
                    this.excelHeaders = cache.excelHeaders || [];
                    this.excelData = cache.excelData || [];
                    this.mapping = cache.mapping || {};
                    this.importResults = cache.importResults || [];

                    if (this.importResults.length > 0) {
                        this.currentTab = 1; // Show errors if any
                    }
                }
            } catch (e) {
                console.error('Load cache failed', e);
            }
        },
        saveCache() {
            try {
                const data = {
                    fileName: this.fileName,
                    sheetNames: this.sheetNames,
                    selectedSheetName: this.selectedSheetName,
                    excelHeaders: this.excelHeaders,
                    excelData: this.excelData,
                    mapping: this.mapping,
                    importResults: this.importResults
                };
                storage.set(CACHE_KEY, data);
            } catch (e) {
                console.error('Save cache failed', e);
            }
        },
        clearCache() {
            try {
                storage.remove(CACHE_KEY);
            } catch (e) {
                console.error('Clear cache failed', e);
            }
        },
        goBack() {
            uni.navigateBack();
        },
        showFullName() {
            if (this.fileName) {
                uni.showToast({ title: this.fileName, icon: 'none' });
            }
        },
        async chooseFile() {
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
                        confirmText: this.$t('itemManage.go_to_settings'),
                        success: (res) => { if (res.confirm) plus.runtime.openAppSettings(); }
                    });
                    return;
                }
            }
            // #endif

            const options = {
                count: 1,
                type: 'file',
                extension: ['.xls', '.xlsx'],
                success: (res) => {
                    this.clearCache();
                    this.processFile(res.tempFiles[0]);
                },
                fail: (err) => {
                    if (err && err.errMsg && err.errMsg.indexOf('cancel') === -1) {
                        uni.showToast({ title: this.$t('itemManage.select_cancel_fail'), icon: 'none' });
                    }
                }
            };

            // #ifdef MP-WEIXIN
            uni.chooseMessageFile(options);
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
            uni.chooseFile({
                extension: ['.xls', '.xlsx'],
                ...options
            });
            // #endif
        },
        androidFilePicker(successCallback) {
            const Intent = plus.android.importClass("android.content.Intent");
            const intent = new Intent(Intent.ACTION_GET_CONTENT);
            intent.setType("*/*"); // Try to pick any, XLS/XLSX filter is harder via intent
            intent.addCategory(Intent.CATEGORY_OPENABLE);
            const main = plus.android.runtimeMainActivity();
            main.onActivityResult = (requestCode, resultCode, data) => {
                if (resultCode === -1 && data) { // -1 is RESULT_OK
                    try {
                        const uri = data.getData();
                        const path = uri ? uri.toString() : '';
                        successCallback({
                            tempFiles: [{
                                path: path,
                                name: 'Selected_File_' + Date.now() + '.xlsx'
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
        processFile(file) {
            this.fileName = file.name;
            const filePath = file.path;

            uni.showLoading({ title: this.$t('itemManage.file_read_in') });

            // #ifdef H5
            const reader = new FileReader();
            reader.onload = (e) => {
                uni.hideLoading();
                const data = new Uint8Array(e.target.result);
                this.parseExcel(data);
            };
            reader.readAsArrayBuffer(file);
            // #endif

            // #ifdef APP-PLUS || MP-WEIXIN
            const timeout = setTimeout(() => {
                uni.hideLoading();
                uni.showToast({ title: this.$t('itemManage.read_timeout'), icon: 'none' });
            }, 15000);

            // Special handling for Android Content URIs using native Java
            if (uni.getSystemInfoSync().platform === 'android') {
                try {
                    const contentUri = filePath;
                    const main = plus.android.runtimeMainActivity();
                    const resolver = plus.android.invoke(main, "getContentResolver");
                    const Uri = plus.android.importClass("android.net.Uri");
                    const uri = Uri.parse(contentUri);
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
                            // Directly use base64 type for better performance
                            this.parseExcel(base64Data, 'base64');
                            return;
                        }
                    }
                } catch (e) {
                    console.error('Native read failed', e);
                    // Don't hide loading yet, try fallback
                }
            }

            const readData = (path) => {
                const fs = uni.getFileSystemManager();
                fs.readFile({
                    filePath: path,
                    success: (res) => {
                        clearTimeout(timeout);
                        uni.hideLoading();
                        try {
                            this.parseExcel(new Uint8Array(res.data));
                        } catch (err) {
                            uni.showToast({ title: this.$t('itemManage.excel_format_fail'), icon: 'none' });
                        }
                    },
                    fail: (err) => {
                        // iOS/fallback plus.io logic
                        // #ifdef APP-PLUS
                        plus.io.resolveLocalFileSystemURL(path, (entry) => {
                            entry.file((fileObj) => {
                                const reader = new plus.io.FileReader();
                                reader.onloadend = (e) => {
                                    clearTimeout(timeout);
                                    uni.hideLoading();
                                    this.parseExcel(new Uint8Array(e.target.result));
                                };
                                reader.readAsArrayBuffer(fileObj);
                            }, () => { clearTimeout(timeout); uni.hideLoading(); uni.showToast({ title: this.$t('itemManage.access_file_fail'), icon: 'none' }); });
                        }, () => {
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

            readData(filePath);
            // #endif
        },
        parseExcel(data, type = 'array') {
            try {
                const workbook = XLSX.read(data, { type: type });
                this.workbook = workbook;
                this.sheetNames = workbook.SheetNames;

                if (this.sheetNames.length > 0) {
                    this.onSheetChange({ detail: { value: 0 } }); // Auto select first
                } else {
                    this.saveCache(); // Save even if just file loaded
                }
            } catch (e) {
                console.error(e);
                uni.showToast({ title: this.$t('itemManage.excel_parse_fail'), icon: 'none' });
            }
        },
        onSheetChange(e) {
            const index = e.detail.value;
            this.selectedSheetName = this.sheetNames[index];

            if (!this.workbook) {
                uni.showToast({ title: this.$t('common.tip'), icon: 'none' });
                return;
            }

            const worksheet = this.workbook.Sheets[this.selectedSheetName];
            // Get JSON
            const jsonData = XLSX.utils.sheet_to_json(worksheet, { header: 1 }); // Header: 1 gives array of arrays

            if (jsonData && jsonData.length > 0) {
                this.excelHeaders = jsonData[0].filter(h => h !== undefined && h !== null && String(h).trim() !== ''); // Clean headers

                // Load persistent mapping first
                const persistMapping = storage.get(PERSIST_MAPPING_KEY) || {};

                // Initialize mapping
                this.mapping = {};

                // 1. First apply persistent mappings if headers exist in current file
                Object.keys(this.sysFields).forEach(key => {
                    const savedHeader = persistMapping[key];
                    if (savedHeader && this.excelHeaders.includes(savedHeader)) {
                        this.$set(this.mapping, key, savedHeader);
                    }
                });

                // 2. For remaining unmapped fields, try auto fuzzy matching
                this.autoMapInfo();

                // Parse Data
                const rawData = XLSX.utils.sheet_to_json(worksheet); // Array of objects keyed by header
                this.excelData = rawData;

                this.saveCache(); // Save state
            }
        },
        autoMapInfo() {
            // Fuzzy match for fields not already mapped
            this.excelHeaders.forEach(header => {
                const headerStr = String(header).trim();
                for (const key in this.sysFields) {
                    // Skip if already mapped by persistence
                    if (this.mapping[key]) continue;

                    const label = this.sysFields[key].label;
                    if (headerStr.includes(label) || label.includes(headerStr)) {
                        this.$set(this.mapping, key, headerStr);
                    }
                }
            });
        },
        getMappingIndex(key) {
            const header = this.mapping[key];
            // +1 because index 0 is '请选择'
            const idx = this.excelHeaders.indexOf(header);
            return idx >= 0 ? idx + 1 : 0;
        },
        onMappingChange(e, key) {
            const index = Number(e.detail.value);
            const persistMapping = storage.get(PERSIST_MAPPING_KEY) || {};

            if (index === 0) {
                // Manual unmap for this session - do NOT delete from persistMapping
                // This allows the user to temporarily skip a field without LOSING the memory
                this.$set(this.mapping, key, null);
            } else {
                const selectedHeader = this.excelHeaders[index - 1];

                // "Replaced" logic: If this Column is already assigned to another field, 
                // clear that field's mapping because it's being "replaced" by the current field.
                Object.keys(persistMapping).forEach(k => {
                    if (persistMapping[k] === selectedHeader && k !== key) {
                        delete persistMapping[k];
                        // If it's also in current session mapping, clear it
                        if (this.mapping[k] === selectedHeader) {
                            this.$set(this.mapping, k, null);
                        }
                    }
                });

                this.$set(this.mapping, key, selectedHeader);
                persistMapping[key] = selectedHeader;
            }

            // Save to both persistent storage and session cache
            storage.set(PERSIST_MAPPING_KEY, persistMapping);
            this.saveCache();
        },
        getPreviewData(key) {
            const header = this.mapping[key];
            if (!header || !this.excelData || this.excelData.length === 0) return '-';
            // Get first 3 rows, check if undefined
            const previews = this.excelData.slice(0, 3).map(row => {
                const val = row[header];
                return (val === undefined || val === null) ? '' : val;
            });
            return previews.join(', ');
        },
        validateData() {
            this.importResults = [];
            const errors = [];

            this.excelData.forEach((row, index) => {
                const rowNum = index + 2; // +1 for 0-index, +1 for header
                const rowErrors = [];

                // Check Required
                for (const key in this.sysFields) {
                    if (this.sysFields[key].required) {
                        const header = this.mapping[key];
                        const val = row[header];
                        if (val === undefined || val === null || String(val).trim() === '') {
                            rowErrors.push(`列 [${header}] 不能为空`);
                        }
                    }
                }

                // Name/FullName Check
                const nameH = this.mapping.Name;
                const fullH = this.mapping.FullName;
                const hasName = nameH && row[nameH] !== undefined && row[nameH] !== null && String(row[nameH]).trim() !== '';
                const hasFull = fullH && row[fullH] !== undefined && row[fullH] !== null && String(row[fullH]).trim() !== '';

                if (!hasName && !hasFull) {
                    return; // Skip empty rows
                }

                // Type Checks (Basic)
                if (rowErrors.length === 0) {
                    // Check Number types
                    const numFields = ['Category', 'Cardinality', 'OrderBy', 'KindCounter', 'AvailableStock', 'Layer', 'LayerNO', 'ColumnNO'];
                    numFields.forEach(f => {
                        if (this.mapping[f]) {
                            const val = row[this.mapping[f]];
                            // Only validate if value exists
                            if (val !== undefined && val !== null && String(val).trim() !== '' && isNaN(Number(val))) {
                                // Special check for Category text
                                if (f === 'Category' && ['药品', '设备', '耗材'].includes(String(val).trim())) {
                                    return;
                                }
                                rowErrors.push(`列 [${this.mapping[f]}] 必须是数字`);
                            }
                        }
                    });
                }

                if (rowErrors.length > 0) {
                    errors.push(`第${rowNum}行: ${rowErrors.join(', ')}`);
                }
            });


            if (errors.length > 0) {
                this.importResults = errors;
                this.currentTab = 1; // Switch to result tab
                uni.showToast({ title: this.$t('itemManage.data_validation_fail'), icon: 'none' });
                this.saveCache();
                return false;
            }

            return true;
        },
        async startImport() {
            if (this.uploading) return;

            // Validate first
            if (!this.validateData()) {
                return;
            }

            this.uploading = true;
            this.totalRows = this.excelData.length;
            this.progress = 0;

            const importList = [];

            // Use index for OrderBy default
            for (let i = 0; i < this.excelData.length; i++) {
                const row = this.excelData[i];
                const payload = {
                    CreatorId: this.userInfo.UserId,
                    Creator: this.userInfo.UserName,
                    IsDeleted: 0,
                    Status: 1,
                    OrganizationId: this.userInfo.OrganizationId || 0,
                    DepartmentId: this.userInfo.DepartmentId || 0
                };

                // Map fields
                for (const key in this.mapping) {
                    const excelHeader = this.mapping[key];
                    if (excelHeader && row[excelHeader] !== undefined) {
                        payload[key] = row[excelHeader];
                    }
                }

                // Skip empty rows
                if ((!payload.Name || String(payload.Name).trim() === '') &&
                    (!payload.FullName || String(payload.FullName).trim() === '')) {
                    continue;
                }

                // Name/FullName Fallback
                if (!payload.Name && payload.FullName) payload.Name = payload.FullName;
                if (!payload.FullName && payload.Name) payload.FullName = payload.Name;

                // Transform Category Text to Value
                if (payload.Category && isNaN(Number(payload.Category))) {
                    const catMap = { '药品': 10, '耗材': 20, '设备': 30 };
                    const txt = String(payload.Category).trim();
                    if (catMap[txt]) {
                        payload.Category = catMap[txt];
                    } else {
                        payload.Category = 0;
                    }
                }

                if (payload.Category === undefined || payload.Category === null || String(payload.Category).trim() === '') {
                    payload.Category = 0;
                }

                // Defaults
                if (payload.Cardinality === undefined || payload.Cardinality === null || String(payload.Cardinality).trim() === '') {
                    payload.Cardinality = 1;
                }

                if (payload.OrderBy === undefined || payload.OrderBy === null || String(payload.OrderBy).trim() === '') {
                    payload.OrderBy = (i + 1) * 10;
                }

                importList.push(payload);
                this.progress++;
            }

            try {
                // Bulk Insert via DetailData
                // Assumed API structure: { detailData: [ ... ] } as per instruction
                let res = await request.post('api/ssbh/Item/Add', { detailData: importList });

                if (res.success || res.status) {
                    this.uploading = false;
                    this.clearCache(); // Success clean up
                    uni.showModal({
                        title: this.$t('itemManage.import_complete'),
                        content: this.$t('itemManage.import_count', { count: importList.length }),
                        showCancel: false,
                        success: () => {
                            uni.navigateBack();
                        }
                    });
                } else {
                    // Import failed or partial success
                    this.uploading = false;
                    this.importResults = [res.message || '导入失败'];
                    if (res.data && Array.isArray(res.data)) {
                        // Assuming res.data might contain list of errors
                        this.importResults = res.data;
                    }
                    this.currentTab = 1;
                    this.saveCache(); // Save error state
                    uni.showToast({ title: this.$t('itemManage.import_exception'), icon: 'none' });
                }
            } catch (e) {
                console.error(e);
                this.uploading = false;
                this.importResults = [' 请求失败: ' + (e.message || JSON.stringify(e))];
                this.currentTab = 1;
                this.saveCache();
            }
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

        .nav-left {
            width: 80rpx;
        }

        .nav-title {
            font-size: 32rpx;
            font-weight: 600;
        }

        .nav-right {
            width: 80rpx;
        }
    }
}

.import-content {
    flex: 1;
    padding: 32rpx;
    width: 100%;
    box-sizing: border-box;
}

.step-card {
    background: #fff;
    border-radius: 24rpx;
    padding: 32rpx;
    margin-bottom: 32rpx;
}

.step-header {
    display: flex;
    align-items: center;
    margin-bottom: 32rpx;
}

.tab-header {
    display: flex;
    background: #fff;
    border-radius: 50rpx;
    padding: 8rpx;
    margin-bottom: 32rpx;
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
    position: relative;
}

.tab-item.active {
    background: $sull-color-primary;
    color: #fff;
    font-weight: 600;
    box-shadow: 0 4rpx 12rpx rgba(149, 202, 255, 0.4);
}

.tab-item.active::after {
    display: none;
}

.badge {
    position: absolute;
    top: 0px;
    right: 20rpx;
    background: #ff4d4f;
    color: #fff;
    font-size: 20rpx;
    height: 32rpx;
    min-width: 32rpx;
    line-height: 32rpx;
    padding: 0 8rpx;
    border-radius: 16rpx;
    display: flex;
    align-items: center;
    justify-content: center;
    transform: none;
}

.step-num {
    width: 48rpx;
    height: 48rpx;
    background: $sull-color-primary;
    color: #fff;
    border-radius: 50%;
    text-align: center;
    line-height: 48rpx;
    font-size: 28rpx;
    margin-right: 16rpx;
}

.step-title {
    font-weight: bold;
    font-size: 32rpx;
}

.file-area {
    height: 160rpx;
    border: 2rpx dashed $sull-color-line;
    border-radius: 16rpx;
    display: flex;
    align-items: center;
    justify-content: center;
    background: #fafafa;

    .upload-placeholder {
        text-align: center;
        color: $sull-text-secondary;
        font-size: 28rpx;
        display: flex;
        flex-direction: column;
        align-items: center;
    }

    .icon {
        font-size: 48rpx;
        margin-bottom: 8rpx;
    }

    .file-info {
        display: flex;
        flex-direction: row;
        align-items: center;
        justify-content: space-between;
        width: 100%;
        padding: 0 32rpx;
        box-sizing: border-box;
    }

    .name {
        font-weight: bold;
        font-size: 28rpx;
        margin-bottom: 0;
        white-space: nowrap;
        overflow: hidden;
        text-overflow: ellipsis;
        max-width: calc(100% - 100rpx);
    }

    .change-btn {
        font-size: 24rpx;
        color: $sull-color-primary;
        white-space: nowrap;
    }
}

.sheet-select {
    margin-top: 32rpx;
}

.sheet-select .label {
    font-size: 28rpx;
    color: #666;
    margin-bottom: 16rpx;
    display: block;
}

.picker-view {
    background: #f5f5f5;
    height: 80rpx;
    border-radius: 8rpx;
    display: flex;
    align-items: center;
    justify-content: space-between;
    padding: 0 24rpx;
    font-size: 28rpx;
}

.mapping-list {
    border: 2rpx solid #eee;
    border-radius: 16rpx;
}

.mapping-row {
    display: flex;
    border-bottom: 2rpx solid #eee;
    min-height: 80rpx;
    align-items: center;
}

.mapping-row:last-child {
    border-bottom: none;
}

.header {
    background: #f9f9f9;
    font-weight: bold;
    font-size: 26rpx;
    color: #666;
}

.col-sys {
    width: 200rpx;
    padding: 16rpx;
    border-right: 2rpx solid #eee;
    flex-shrink: 0;
}

.col-excel {
    width: 240rpx;
    padding: 16rpx;
    border-right: 2rpx solid #eee;
    flex-shrink: 0;
}

.col-preview {
    flex: 1;
    padding: 16rpx;
    color: #999;
    font-size: 24rpx;
    overflow: hidden;
}

.preview-text {
    display: -webkit-box;
    -webkit-box-orient: vertical;
    -webkit-line-clamp: 2;
    line-clamp: 2;
    overflow: hidden;
}

.field-label {
    font-size: 28rpx;
}

.required-mark {
    color: red;
    margin-left: 4rpx;
}

.picker-cell {
    font-size: 28rpx;
    color: #333;
}

.picker-cell.empty {
    color: #999;
}

.action-area {
    padding: 32rpx 0;
}

.submit-btn {
    background: $sull-color-primary;
    color: #fff;
    border-radius: 48rpx;
    font-size: 32rpx;
    height: 96rpx;
    line-height: 96rpx;
}

.submit-btn[disabled] {
    opacity: 0.6;
}

.result-list {
    min-height: 400rpx;
}

.result-empty {
    padding: 80rpx 0;
    text-align: center;
    color: #999;
    font-size: 28rpx;
}

.result-item {
    padding: 16rpx 24rpx;
    border-bottom: 2rpx solid #f5f5f5;
}

.error-msg {
    color: #ff4d4f;
    font-size: 26rpx;
}
</style>
