export function getRescueCarStatusText(status, $t) {
    // If $t is not provided, return a default string (or handle error)
    // We expect $t to be passed from the component context (this.$t)
    const t = $t || ((key) => null);

    const statusMap = {
        0: t('bigScreen.rescueCar.statusNormal') || '正常',
        10: t('bigScreen.rescueCar.statusAbnormal') || '异常',
        11: t('bigScreen.rescueCar.statusConfirming') || '确认中',
        20: t('bigScreen.rescueCar.statusDirty') || '脏污',
        21: t('bigScreen.rescueCar.statusCleaning') || '清洁中',
        30: t('bigScreen.rescueCar.statusRepair') || '报修',
        31: t('bigScreen.rescueCar.statusFixing') || '维修中'
    };
    return statusMap[status] || t('bigScreen.rescueCar.statusNormal') || '正常';
}

export const RESCUE_CAR_STATUS = {
    NORMAL: 0,
    ABNORMAL: 10,
    CONFIRMING: 11,
    DIRTY: 20,
    CLEANING: 21,
    REPAIR: 30,
    FIXING: 31
};
