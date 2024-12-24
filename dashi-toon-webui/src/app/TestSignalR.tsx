"use client";
import { useSignalR } from "@/hooks/useSignalR";
import React, { useCallback } from "react";
import * as env from "@/utils/env";

export default function TestSignalR() {
    const [msgs, setMsgs] = React.useState<string[]>([]);
    const [msg, setMsg] = React.useState<string>("");
    useSignalR("HelloConcac", (_, msg) => {
        setMsgs((msgs) => [...msgs, msg]);
    });

    const handleSend = () => {
        fetch(`${env.getBackendHost()}/api/Konichiwassup?message=${msg}`, {
            method: "POST",
        });
    };

    return (
        <div>
            <button onClick={handleSend}>Click me to send stuff</button>
            <input
                type="text"
                value={msg}
                onChange={(e) => setMsg(e.target.value)}
                style={{ color: "black" }}
            />
            <h2>Messages</h2>
            <ol>
                {msgs.map((msg, i) => (
                    <li key={i}>{msg}</li>
                ))}
            </ol>
        </div>
    );
}
