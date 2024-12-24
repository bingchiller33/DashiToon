import react, { useContext, useEffect } from "react";
import { SignalRContext } from "@/components/SignalRProvider";
import { HubConnection } from "@microsoft/signalr";
export function useSignalR(
    proc: string,
    handler: (hub: HubConnection, ...args: any[]) => any
) {
    const conn = useContext(SignalRContext);
    useEffect(() => {
        if (conn === undefined) {
            return;
        }

        const ex = (...args: any[]) => {
            handler(conn, ...args);
        };

        conn.on(proc, ex);

        return () => {
            conn.off(proc, ex);
        };
    }, [conn, proc, handler]);
}
