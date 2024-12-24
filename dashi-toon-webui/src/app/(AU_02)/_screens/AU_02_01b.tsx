"use client";

import { validateEmail } from "@/utils";
import ToastService from "@/utils/service/ToastService";
import { useSearchParams } from "next/navigation";
import React, { useEffect, useState } from "react";
import * as env from "@/utils/env";

export default function AU_02_01bScreen(props: AU_02_01bScreenProps) {
    const searchParams = useSearchParams();
    const email = searchParams.get("email");

    const [cd, setCd] = useState<number>(0);

    useEffect(() => {
        let timer: NodeJS.Timeout;
        if (cd > 0) {
            timer = setTimeout(() => setCd(cd - 1), 1000);
        }
        return () => clearTimeout(timer);
    }, [cd]);

    const handleResend = async () => {
        if (!email || !validateEmail(email)) {
            ToastService.showToast("6001", ["email"]);
            return;
        }

        try {
            const response = await fetch(
                `${env.getBackendHost()}/api/Users/forgotPassword`,
                {
                    method: "POST",
                    headers: {
                        "Content-Type": "application/json",
                    },
                    body: JSON.stringify({ email }),
                },
            );

            if (response.ok) {
                ToastService.showToast("4001", ["forgot password"]);
                setCd(60);
            } else {
                ToastService.showToast("6001", ["email"]);
            }
        } catch (err) {
            ToastService.showToast("8002", []);
        }
    };

    return (
        <div className="flex h-screen justify-center">
            <div className="w-80 pt-40 lg:w-96">
                <h1 className="mb-4 text-2xl font-bold text-white">
                    Xác nhận quên mật khẩu
                </h1>
                <h1>
                    Vui lòng kiểm tra email của bạn và làm theo hướng dẫn để đặt
                    lại mật khẩu. Nếu bạn không nhận được email, vui lòng kiểm
                    tra thư mục spam.{" "}
                    {cd > 0 ? (
                        <>
                            Bạn có thể gửi lại sau
                            <span className="text-gray-500"> {cd}</span> giây.
                        </>
                    ) : (
                        <>
                            Hoặc{" "}
                            <span
                                onClick={handleResend}
                                className="cursor-pointer text-blue-500 hover:underline"
                            >
                                {" "}
                                nhấn vào đây
                            </span>{" "}
                            để gửi lại.
                        </>
                    )}
                </h1>
            </div>
        </div>
    );
}

export interface AU_02_01bScreenProps {
    test: boolean;
    returnPath?: string;
}
