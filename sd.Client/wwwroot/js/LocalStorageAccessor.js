export function get(key) {
    const value = window.localStorage.getItem(key);
    return value ? JSON.parse(value) : null;
}

export function set(key, value) {
    window.localStorage.setItem(key, value);
}

export function clear() {
    window.localStorage.clear();
}

export function remove(key) {
    window.localStorage.removeItem(key);
}