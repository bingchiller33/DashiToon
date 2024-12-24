export type Result<T, E = Object> = [T, undefined?] | [undefined, E];
export interface OkPromise<T> extends Promise<T> {}

export function errAsValue<T, E = Object>(fn: () => T): Result<T, E> {
    try {
        const rs = fn();
        return [rs, undefined];
    } catch (e) {
        return [undefined, e as E];
    }
}

export function promiseErr<T, E = Object>(
    prom: Promise<T>
): OkPromise<Result<T, E>> {
    return new Promise(async (resolve) => {
        try {
            resolve([await prom, undefined]);
        } catch (e) {
            resolve([undefined, e as E]);
        }
    });
}

export function hp<T, R extends T>(work: (short: (ret?: R) => void) => T): T {
    try {
        return work(shortCircuit);
    } catch (e: any) {
        if (e.__hp_short) {
            return e.__hp_returnVal as T;
        }

        throw e;
    }
}

function shortCircuit<T>(returnVal?: T) {
    throw { __hp_short: true, __hp_returnVal: returnVal };
}
