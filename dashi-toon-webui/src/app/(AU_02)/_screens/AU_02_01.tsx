"use client";

import React from "react";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { validateEmail } from "@/utils";
import MessageService from "@/utils/service/MessageService";
import ToastService from "@/utils/service/ToastService";
import * as env from "@/utils/env";
import Link from "next/link";

export default function AU_02_01Screen(props: AU_02_01ScreenProps) {
    const [email, setEmail] = React.useState("");
    const [errorMsg, setErrorMsg] = React.useState<string>("");

    const handleEmailBlur = () => {
        if (!email) {
            const errorMsg = MessageService.get("6004", ["Email"]);
            setErrorMsg(errorMsg);
            return;
        } else if (!validateEmail(email)) {
            const errorMsg = MessageService.get("6001", ["email"]);
            setErrorMsg(errorMsg);
            return;
        } else {
            setErrorMsg("");
        }
    };

    const handleForgot = async (e: React.FormEvent<HTMLFormElement>) => {
        e.preventDefault();
        if (!email) {
            ToastService.showToast("6004", ["Email"]);
            return;
        } else if (!validateEmail(email)) {
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
                window.location.href = `/forgot-password-confirmation?email=${encodeURIComponent(
                    email,
                )}`;
                ToastService.showToast("4001", ["forgot password"]);
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
                {" "}
                <h1 className="text-2xl">Quên mật khẩu?</h1>
                <h1 className="text-2xl">Nhập email của bạn.</h1>
                <form onSubmit={handleForgot}>
                    <hr className="m-auto mt-4 w-16 border border-t-2 border-gray-500" />
                    <div className="mt-4">
                        <div className="mb-1 flex items-center justify-between">
                            <Label htmlFor="email">Email</Label>
                            {errorMsg && (
                                <span className="ml-1 text-xs text-red-500">
                                    {errorMsg}
                                </span>
                            )}
                        </div>
                        <Input
                            name="email"
                            placeholder="Email"
                            className="w-full"
                            onChange={(e) => setEmail(e.target.value)}
                            onBlur={handleEmailBlur}
                            value={email}
                        />
                    </div>
                    <Button
                        type="submit"
                        className="mb-4 mt-6 w-full bg-[#00a3ff] text-white hover:bg-[#00a3ff]/80"
                    >
                        Đặt Lại Mật Khẩu
                    </Button>
                </form>
                <span>
                    Đã nhớ thông tin đăng nhập?{" "}
                    <Link
                        className="cursor-pointer text-blue-500 hover:underline"
                        href="/login"
                    >
                        Đăng nhập
                    </Link>
                </span>
            </div>
        </div>
    );
}

export interface AU_02_01ScreenProps {
    test: boolean;
    returnPath?: string;
}
