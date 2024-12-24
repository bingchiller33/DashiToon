import { Result } from "@/utils/err";
import { useEffect, useState } from "react";
import { toast } from "sonner";

type FetchFn<I extends Array<any>, T> = (...args: I) => Promise<Result<T>>;

export interface UseFetchOptions<T> {
    msg?: string;
    onSuccess?: (data: T) => void;
}

export function useFetchApi<I extends Array<any>, T>(fn: FetchFn<I, T>, deps?: I, opts?: UseFetchOptions<T>) {
    const [isLoading, setLoading] = useState(true);
    const [data, setData] = useState<T>();

    const realDeps = deps ?? ([] as unknown as I);

    useEffect(() => {
        let mounted = true;

        async function work() {
            setLoading(true);
            const [data, err] = await fn(...realDeps);
            if (!mounted) {
                return;
            }

            if (err) {
                if (opts?.msg) toast.error(opts.msg);

                return;
            }

            setData(data);
            if (opts?.onSuccess) {
                opts.onSuccess(data);
            }
            setLoading(false);
        }

        work();

        return () => {
            mounted = false;
        };
    }, deps ?? []);

    return [data, isLoading] as const;
}
