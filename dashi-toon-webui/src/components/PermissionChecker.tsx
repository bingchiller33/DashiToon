"use client";
import { getUserInfo, UserSession } from "@/utils/api/user";
import React, { useEffect, useState } from "react";
import { redirect } from "next/navigation";
import { LoadingSpinner } from "./LoadingSpinenr";

export interface PermissionCheckerProps {
    children: React.ReactNode;
    allowedRoles?: string[];
    hiddenUntilLoaded?: boolean;
    customCheckLogic?: (
        session: UserSession,
        roleCheck: boolean,
    ) => Promise<boolean>;
}
export default function PermissionChecker({
    children,
    allowedRoles,
    hiddenUntilLoaded,
    customCheckLogic,
}: PermissionCheckerProps) {
    const [session, setSession] = useState<UserSession | null>(null);
    const [isLoaded, setIsLoaded] = useState(false);
    const [allowed, setAllowed] = useState(false);

    useEffect(() => {
        async function work() {
            if (!allowedRoles || allowedRoles.length === 0) {
                setAllowed(true);
                setIsLoaded(true);
                return;
            }

            const [s, e] = await getUserInfo();
            if (e) {
                setSession(null);
                return;
            }

            setSession(s);
            setIsLoaded(true);

            let allowed = false;
            for (const role of s.roles) {
                if (allowedRoles.includes(role)) {
                    allowed = true;
                    break;
                }
            }

            if (customCheckLogic)
                setAllowed(await customCheckLogic(s, allowed));
            else setAllowed(allowed);
        }

        work();
    }, [allowedRoles, customCheckLogic]);

    if (isLoaded) {
        if (allowed) {
            return <>{children}</>;
        } else {
            redirect("/not-found");
        }
    } else {
        if (hiddenUntilLoaded) {
            return (
                <div className="flex flex-grow items-center justify-center">
                    <LoadingSpinner
                        size={64}
                        className="text-blue-500 opacity-75 transition-opacity ease-in-out"
                    />
                </div>
            );
        } else {
            return <>{children}</>;
        }
    }
}
