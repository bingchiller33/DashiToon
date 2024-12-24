"use client";

import React from "react";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Button } from "@/components/ui/button";
import Link from "next/link";
import googleLogo from "../../../../public/images/google.png";
import * as env from "@/utils/env";
import Image from "next/image";
import ToastService from "@/utils/service/ToastService";
import { validateEmail } from "@/utils";

export default function AU_01_01Screen(props: AU_01_01ScreenProps) {
    const [email, setEmail] = React.useState("");
    const [password, setPassword] = React.useState("");

    const handleLogin = () => {
        console.log("asdad");

        if (!validateEmail(email)) {
            ToastService.showToast("6001", ["email"]);
            return;
        }

        fetch(`${env.getBackendHost()}/api/Users/login?useCookies=true`, {
            method: "POST",
            credentials: "include",
            headers: {
                "Content-Type": "application/json",
            },
            body: JSON.stringify({
                email,
                password,
            }),
        })
            .then((x) => {
                console.log(x);
                if (x.ok) {
                    window.location.href = props.returnPath ?? "/";
                } else if (x.status === 401) {
                    ToastService.showToast("6002", []);
                } else {
                    ToastService.showToast("8002", []);
                }
            })
            .catch((x) => {
                ToastService.showToast("8002", []);
                console.error(x);
            });
    };

    const { test, returnPath } = props;
    return (
        <div className="flex h-screen justify-center">
            <form className="w-96 pt-40 sm:w-96">
                <h1 className="text-2xl">Đăng nhập</h1>
                <div className="grid w-full items-center gap-1.5 pt-6">
                    <Label htmlFor="email">Email</Label>
                    <Input
                        className="w-full"
                        type="email"
                        id="email"
                        placeholder="Email"
                        value={email}
                        onChange={(e) => setEmail(e.target.value)}
                    />
                </div>

                <div className="grid w-full items-center gap-1.5 pt-6">
                    <div className="flex items-center justify-between">
                        <Label htmlFor="password">Mật khẩu</Label>
                        <Link
                            className="text-sm text-[#d3cfcf]"
                            href="/forgot-password"
                        >
                            Quên mật khẩu?
                        </Link>
                    </div>
                    <Input
                        className="w-full"
                        type="password"
                        id="password"
                        placeholder="Mật khẩu"
                        value={password}
                        onChange={(e) => setPassword(e.target.value)}
                    />
                </div>

                <Button
                    className="mt-6 w-full bg-[#00a3ff] text-white"
                    onClick={handleLogin}
                    type="button"
                >
                    Đăng nhập
                </Button>

                <p className="py-8 text-center">
                    Bạn chưa có tài khoản?{" "}
                    <Link className="text-[#429ff5]" href="/register">
                        Đăng ký
                    </Link>
                </p>

                <div className="flex items-center gap-2">
                    <div className="h-[1px] flex-1 bg-white"></div>
                    <p>Hoặc</p>
                    <div className="h-[1px] flex-1 bg-white"></div>
                </div>

                <Button asChild className="mt-6 w-full">
                    <Link
                        className="mt-6 w-full"
                        href={`${env.getBackendHost()}/api/Users/signin-google?returnUrl=${returnPath}`}
                    >
                        <Image
                            src={googleLogo}
                            alt="logo google"
                            className="h-6 w-6"
                        />{" "}
                        Tiếp tục với Google
                    </Link>
                </Button>
            </form>
        </div>
    );
}

export interface AU_01_01ScreenProps {
    test: boolean;
    returnPath?: string;
}
