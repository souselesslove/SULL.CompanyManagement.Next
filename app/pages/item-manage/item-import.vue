<template>
    <view class="container">
        <!-- Navigation Bar -->
        <view class="custom-nav" :style="{ paddingTop: statusBarHeight + 'px' }">
            <view class="nav-content">
                <view class="nav-left">
                    <BackButton class="nav-back-btn" @click="goBack" />
                </view>
                <text class="nav-title">{{ $t('itemManage.import_title') }}</text>
                <view class="nav-right"></view>
            </view>
        </view>

        <scroll-view class="import-content" scroll-y :style="{ marginTop: (statusBarHeight + 44) + 'px' }">

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
                        上传结果
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
                                    {{ mapping[key] || '请选择' }}
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
import * as XLSX from 'xlsx';

const CACHE_KEY = 'ITEM_IMPORT_CACHE';

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

            // System Fields Definition
            sysFields: {
                Name: { label: '物品名称', required: false }, // Custom check
                FullName: { label: '完整名称', required: false }, // Custom check
                InterfaceCode: { label: '接口代码', required: false }, // Nullable true in schema
                Category: { label: '类型', required: false }, // Default to 0
                Specification: { label: '规格', required: false },
                Unit: { label: '单位', required: false },
                Form: { label: '剂型', required: false },
                Price: { label: '价格（分）', required: false },
                RegisterCode: { label: '批准文号', required: false },
                Factory: { label: '生产厂家', required: false },
                Cardinality: { label: '基数', required: false }, // Default to 1
                Barcode: { label: '条码号', required: false },
                OrderBy: { label: '排序', required: false }, // Default to (index+1)*10
                SearchCode: { label: '检索码', required: false },
                Remark: { label: '备注', required: false },
                Tag: { label: '标签', required: false }
            },

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
        displayExcelHeaders() {
            return ['请选择', ...this.excelHeaders];
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
        const uInfo = uni.getStorageSync('userOrgInfo');
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
                const cache = uni.getStorageSync(CACHE_KEY);
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
                uni.setStorageSync(CACHE_KEY, data);
            } catch (e) {
                console.error('Save cache failed', e);
            }
        },
        clearCache() {
            try {
                uni.removeStorageSync(CACHE_KEY);
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
        chooseFile() {
            // uni.chooseFile works on H5 and some MP. 
            // For WeChat MP, use wx.chooseMessageFile.
            // We'll try uni.chooseFile first or fallback.

            //#ifdef MP-WEIXIN
            wx.chooseMessageFile({
                count: 1,
                type: 'file',
                extension: ['xls', 'xlsx'],
                success: (res) => {
                    this.clearCache(); // Start fresh
                    this.processFile(res.tempFiles[0]);
                }
            });
            //#endif

            //#ifndef MP-WEIXIN
            uni.chooseFile({
                count: 1,
                extension: ['.xls', '.xlsx'],
                success: (res) => {
                    this.clearCache(); // Start fresh
                    this.processFile(res.tempFiles[0]);
                }
            });
            //#endif
        },
        processFile(file) {
            this.fileName = file.name;

            // Read File
            // On H5: file.path is blob url or object. FileReader needed.
            // On MP: file.path is tmp path. FileSystemManager needed.

            //#ifdef H5
            const reader = new FileReader();
            reader.onload = (e) => {
                const data = new Uint8Array(e.target.result);
                this.parseExcel(data);
            };
            reader.readAsArrayBuffer(file); // Note: chooseFile in H5 might return File object directly in tempFiles[0]
            //#endif

            //#ifdef MP-WEIXIN
            const fs = wx.getFileSystemManager();
            fs.readFile({
                filePath: file.path,
                success: (res) => {
                    this.parseExcel(new Uint8Array(res.data));
                }
            });
            //#endif
        },
        parseExcel(data) {
            try {
                const workbook = XLSX.read(data, { type: 'array' });
                this.workbook = workbook;
                this.sheetNames = workbook.SheetNames;

                if (this.sheetNames.length > 0) {
                    this.onSheetChange({ detail: { value: 0 } }); // Auto select first
                } else {
                    this.saveCache(); // Save even if just file loaded
                }
            } catch (e) {
                console.error(e);
                uni.showToast({ title: 'Excel解析失败', icon: 'none' });
            }
        },
        onSheetChange(e) {
            const index = e.detail.value;
            this.selectedSheetName = this.sheetNames[index];

            if (!this.workbook) {
                uni.showToast({ title: '请重新选择文件以切换工作表', icon: 'none' });
                return;
            }

            const worksheet = this.workbook.Sheets[this.selectedSheetName];
            // Get JSON
            const jsonData = XLSX.utils.sheet_to_json(worksheet, { header: 1 }); // Header: 1 gives array of arrays

            if (jsonData && jsonData.length > 0) {
                this.excelHeaders = jsonData[0]; // First row is header
                // Auto mapping based on similarity?
                this.autoMapInfo();

                // Parse Data
                const rawData = XLSX.utils.sheet_to_json(worksheet); // Array of objects keyed by header
                this.excelData = rawData;

                this.saveCache(); // Save state
            }
        },
        autoMapInfo() {
            // Simple auto map
            this.mapping = {};
            this.excelHeaders.forEach(header => {
                for (const key in this.sysFields) {
                    if (header.includes(this.sysFields[key].label) || this.sysFields[key].label.includes(header)) {
                        this.mapping[key] = header;
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
            if (index === 0) {
                this.$set(this.mapping, key, null);
            } else {
                this.$set(this.mapping, key, this.excelHeaders[index - 1]);
            }
            this.saveCache(); // Save mapping change
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
                    const numFields = ['Category', 'Cardinality', 'OrderBy', 'KindCounter', 'AvailableStock'];
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
                uni.showToast({ title: '数据校验未通过', icon: 'none' });
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
                        title: '导入完成',
                        content: `成功导入 ${importList.length} 条数据`,
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
                    uni.showToast({ title: '导入出现异常', icon: 'none' });
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
    box-shadow: 0 1px 4px rgba(0, 0, 0, 0.03);

    .nav-content {
        height: 44px;
        display: flex;
        align-items: center;
        justify-content: space-between;
        padding: 0 16px;

        .nav-left {
            width: 40px;
        }

        .nav-title {
            font-size: 16px;
            font-weight: 600;
        }

        .nav-right {
            width: 40px;
        }
    }
}

.import-content {
    flex: 1;
    padding: 16px;
    width: 100%;
    box-sizing: border-box;
}

.step-card {
    background: #fff;
    border-radius: 12px;
    padding: 16px;
    margin-bottom: 16px;
}

.step-header {
    display: flex;
    align-items: center;
    margin-bottom: 16px;
}

.tab-header {
    display: flex;
    background: #fff;
    border-radius: 25px;
    padding: 4px;
    margin-bottom: 16px;
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
    position: relative;
}

.tab-item.active {
    background: $sull-color-primary;
    color: #fff;
    font-weight: 600;
    box-shadow: 0 2px 6px rgba(149, 202, 255, 0.4);
}

.tab-item.active::after {
    display: none;
}

.badge {
    position: absolute;
    top: 0px;
    right: 10px;
    background: #ff4d4f;
    color: #fff;
    font-size: 10px;
    height: 16px;
    min-width: 16px;
    line-height: 16px;
    padding: 0 4px;
    border-radius: 8px;
    display: flex;
    align-items: center;
    justify-content: center;
    transform: none;
}

.step-num {
    width: 24px;
    height: 24px;
    background: $sull-color-primary;
    color: #fff;
    border-radius: 50%;
    text-align: center;
    line-height: 24px;
    font-size: 14px;
    margin-right: 8px;
}

.step-title {
    font-weight: bold;
    font-size: 16px;
}

.file-area {
    height: 80px;
    border: 1px dashed $sull-color-line;
    border-radius: 8px;
    display: flex;
    align-items: center;
    justify-content: center;
    background: #fafafa;

    .upload-placeholder {
        text-align: center;
        color: $sull-text-secondary;
        font-size: 14px;
        display: flex;
        flex-direction: column;
        align-items: center;
    }

    .icon {
        font-size: 24px;
        margin-bottom: 4px;
    }

    .file-info {
        display: flex;
        flex-direction: row;
        align-items: center;
        justify-content: space-between;
        width: 100%;
        padding: 0 16px;
        box-sizing: border-box;
    }

    .name {
        font-weight: bold;
        font-size: 14px;
        margin-bottom: 0;
        white-space: nowrap;
        overflow: hidden;
        text-overflow: ellipsis;
        max-width: calc(100% - 50px);
    }

    .change-btn {
        font-size: 12px;
        color: $sull-color-primary;
        white-space: nowrap;
    }
}

.sheet-select {
    margin-top: 16px;
}

.sheet-select .label {
    font-size: 14px;
    color: #666;
    margin-bottom: 8px;
    display: block;
}

.picker-view {
    background: #f5f5f5;
    height: 40px;
    border-radius: 4px;
    display: flex;
    align-items: center;
    justify-content: space-between;
    padding: 0 12px;
    font-size: 14px;
}

.mapping-list {
    border: 1px solid #eee;
    border-radius: 8px;
}

.mapping-row {
    display: flex;
    border-bottom: 1px solid #eee;
    min-height: 40px;
    align-items: center;
}

.mapping-row:last-child {
    border-bottom: none;
}

.header {
    background: #f9f9f9;
    font-weight: bold;
    font-size: 13px;
    color: #666;
}

.col-sys {
    width: 100px;
    padding: 8px;
    border-right: 1px solid #eee;
    flex-shrink: 0;
}

.col-excel {
    width: 120px;
    padding: 8px;
    border-right: 1px solid #eee;
    flex-shrink: 0;
}

.col-preview {
    flex: 1;
    padding: 8px;
    color: #999;
    font-size: 12px;
    overflow: hidden;
}

.preview-text {
    display: -webkit-box;
    -webkit-box-orient: vertical;
    -webkit-line-clamp: 2;
    overflow: hidden;
}

.field-label {
    font-size: 14px;
}

.required-mark {
    color: red;
    margin-left: 2px;
}

.picker-cell {
    font-size: 14px;
    color: #333;
}

.picker-cell.empty {
    color: #999;
}

.action-area {
    padding: 16px 0;
}

.submit-btn {
    background: $sull-color-primary;
    color: #fff;
    border-radius: 24px;
    font-size: 16px;
    height: 48px;
    line-height: 48px;
}

.submit-btn[disabled] {
    opacity: 0.6;
}

.result-list {
    min-height: 200px;
}

.result-empty {
    padding: 40px 0;
    text-align: center;
    color: #999;
    font-size: 14px;
}

.result-item {
    padding: 8px 12px;
    border-bottom: 1px solid #f5f5f5;
}

.error-msg {
    color: #ff4d4f;
    font-size: 13px;
}
</style>
