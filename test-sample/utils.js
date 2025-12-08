/**
 * Utility functions for string manipulation
 */

/**
 * Capitalize the first letter of a string
 * @param {string} str - The input string
 * @returns {string} The capitalized string
 */
export function capitalize(str) {
    if (!str || typeof str !== 'string') {
        return '';
    }
    return str.charAt(0).toUpperCase() + str.slice(1);
}

/**
 * Reverse a string
 * @param {string} str - The input string
 * @returns {string} The reversed string
 */
export function reverse(str) {
    if (!str || typeof str !== 'string') {
        return '';
    }
    return str.split('').reverse().join('');
}

/**
 * Count words in a string
 * @param {string} str - The input string
 * @returns {number} The word count
 */
export function countWords(str) {
    if (!str || typeof str !== 'string') {
        return 0;
    }
    return str.trim().split(/\s+/).length;
}
