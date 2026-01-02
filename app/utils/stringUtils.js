/**
 * Calculates string length where Chinese characters (and other full-width characters) count as 2 
 * and standard ASCII characters count as 1.
 * @param {string} str The string to measure
 * @returns {number} The calculated length
 */
export const getStringLen = (str) => {
    if (!str) return 0;
    let len = 0;
    for (let i = 0; i < str.length; i++) {
        const c = str.charCodeAt(i);
        // Single-byte characters: ASCII and half-width katakana
        if ((c >= 0x0001 && c <= 0x007e) || (0xff60 <= c && c <= 0xff9f)) {
            len++;
        } else {
            len += 1.67;
        }
    }
    return parseInt(len);
};
