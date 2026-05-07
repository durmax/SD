export function get(key) {
    const value = window.localStorage.getItem(key);

    if (value === null || value === undefined) {
        return null;
    }

    try {
        return JSON.parse(value);
    } catch {
        return value;
    }
}

export function set(key, value) {
    window.localStorage.setItem(key, JSON.stringify(value));
}

export function clear() {
    window.localStorage.clear();
}

export function remove(key) {
    window.localStorage.removeItem(key);
}