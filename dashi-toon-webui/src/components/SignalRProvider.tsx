"use client";
import React, { PropsWithChildren, useEffect } from "react";
import * as env from "@/utils/env";
import * as signalR from "@microsoft/signalr";

export const SignalRContext = React.createContext<
    signalR.HubConnection | undefined
>(undefined);

export default function SignalRProvider({ children }: PropsWithChildren<{}>) {
    const [connection, setConnection] = React.useState<signalR.HubConnection>();
    useEffect(() => {
        console.info("Setting up SignalR");
        const connection = new signalR.HubConnectionBuilder()
            .withUrl(`${env.SIGNALR_URL}`)
            .withAutomaticReconnect()
            .build();

        connection.start().then(() => {
            console.info("SignalR connected");
            setConnection(connection);
        });

        return () => {
            console.info("Unmounting SignalR");
            connection.stop();
        };
    }, []);

    return (
        <SignalRContext.Provider value={connection}>
            {children}
        </SignalRContext.Provider>
    );
}
