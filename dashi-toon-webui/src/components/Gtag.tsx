"use client";

import Script from "next/script";
import React, { Suspense, useState } from "react";
import * as env from "@/utils/env";
import { useEffect } from "react";
import { usePathname, useSearchParams } from "next/navigation";
import { getUserInfo, UserSession } from "@/utils/api/user";

export function GtagInner() {
    const pathname = usePathname();
    const searchParams = useSearchParams();
    const [session, setSession] = useState<UserSession | null>(null);
    useEffect(() => {
        async function work() {
            const [s, e] = await getUserInfo();
            if (e) {
                setSession(null);
                return;
            }

            setSession(s);
        }

        work();
    }, []);

    useEffect(() => {
        const gtag = (window as any).gtag;
        if (gtag === "function") {
            const url =
                pathname + (searchParams ? `?${searchParams.toString()}` : "");
            gtag("config", env.GA_ID, {
                page_path: url,
                user_id: session?.userId,
            });
        }
    }, [pathname, searchParams, session]);

    return (
        <>
            {/* gtag.js script */}
            <Script
                id="gtag-init"
                strategy="afterInteractive"
                dangerouslySetInnerHTML={{
                    __html: `
              window.dataLayer = window.dataLayer || [];
              function gtag(){dataLayer.push(arguments);}
              gtag('js', new Date());
              gtag('config', '${env.GA_ID}', {
                page_path: window.location.pathname,
              });
            `,
                }}
            />
            <Script
                src={`https://www.googletagmanager.com/gtag/js?id=${env.GA_ID}`}
                strategy="afterInteractive"
            />
        </>
    );
}

export default function Gtag() {
    return (
        <Suspense>
            <GtagInner />
        </Suspense>
    );
}
