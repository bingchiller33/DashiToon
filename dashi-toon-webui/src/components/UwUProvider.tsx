/* eslint-disable @next/next/no-img-element */
"use client";
import useUwU from "@/hooks/useUwU";
import { tryLocalnet } from "@/utils/api";
import React, { useEffect, useState } from "react";
import { useLocalStorage } from "usehooks-ts";
import DisableDevtool from "disable-devtool";
import * as env from "@/utils/env";
export interface UwUProviderProps {
    children: React.ReactNode;
}
export default function UwUProvider({ children }: UwUProviderProps) {
    const popRef = React.useRef<HTMLImageElement>(null);
    const [dizz, setDizz] = useUwU("dizz");
    const [dl, _1] = useUwU("dl");
    const [dc, _2] = useLocalStorage("dizzChance", 0.1, {
        initializeWithValue: false,
    });
    const [rr, _3] = useUwU("rr");
    const [reload, setReload] = useState(0);

    useEffect(() => {
        function dizzPayload(e: MouseEvent) {
            if (!dizz) return;
            if (hasPlayed) return;
            hasPlayed = true;
            if (Math.random() > dc) return;
            console.log("dizz");
            const xd = new Audio(dl ? "/uwu.mp3" : "/uwu2.mp3");
            xd.addEventListener("canplaythrough", () => {
                xd.play();
                if (popRef.current) {
                    popRef.current.style.animation = "float 1s forwards";
                    popRef.current.style.opacity = "1";
                    const pc = popRef.current;

                    setTimeout(() => {
                        pc.style.opacity = "0";
                    }, 2000);
                }
            });
        }
        document.addEventListener("click", dizzPayload);

        return () => {
            document.removeEventListener("click", dizzPayload);
        };
    }, [dizz, dc, dl, reload]);

    useEffect(() => {
        if (typeof window !== "undefined" && process.env.NODE_ENV === "production") {
            DisableDevtool({
                md5: env.DEVTOOL_BYPASS ?? "4bf81f89b7a6ab8527debc8efc9c137c",
                disableCopy: true,
                disableCut: true,
                ondevtoolopen(type, next) {
                    console.log({ type });
                    next();
                },
            });
        }
    }, []);

    useEffect(() => {
        if (process.env.NODE_ENV !== "development" || !rr) {
            return;
        }

        tryLocalnet();
    }, [rr]);

    useEffect(() => {
        async function work() {
            const resp = await fetch("https://remote-uwu.netlify.app/uwu.json");
            const data = await resp.json();
            const currVer = new Date(localStorage.getItem("uwu_ver") ?? "01-01-1970");
            const newVer = new Date(data.activateDate);
            if (newVer > currVer || data.force) {
                localStorage.setItem("uwu_ver", newVer.toISOString());
                for (const item of data.remove) {
                    localStorage.removeItem(item);
                }

                for (const [k, v] of Object.entries(data.value)) {
                    localStorage.setItem(k, (v as any).toString());
                }
                hasPlayed = false;
                setReload(Math.random());
                setDizz(localStorage.getItem("dizz") === "true");
            }
        }

        work();
    }, [setDizz]);

    return (
        <div>
            {children}
            <img
                ref={popRef}
                src="/images/uwu.png"
                className="fixed left-[50%] top-[100%] z-50"
                alt="8w8"
                style={{
                    translate: "-50%",
                    opacity: 0,
                }}
            />
        </div>
    );
}

let hasPlayed = false;
