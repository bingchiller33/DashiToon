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
import { validateEmail, validatePassword } from "@/utils";
import MessageService from "@/utils/service/MessageService";

export default function AU_03_01Screen(props: AU_03_01ScreenProps) {
    const [isRegistered, setIsRegistered] = React.useState(false);
    const [form, setForm] = React.useState({
        username: {
            value: "",
            validateMessage: "",
        },
        email: {
            value: "",
            validateMessage: "",
        },
        password: {
            value: "",
            validateMessage: "",
        },
        confirmPassword: {
            value: "",
            validateMessage: "",
        },
    });

    const handleOnChangeForm = (item: string, value: string) => {
        let validateMessage = "";
        switch (item) {
            case "username": {
                if (!value) {
                    validateMessage = MessageService.get("6004", ["Username"]);
                }
                break;
            }
            case "email": {
                if (!value) {
                    validateMessage = MessageService.get("6004", [
                        "Email Address",
                    ]);
                } else if (!validateEmail(value)) {
                    validateMessage = MessageService.get("6001", [
                        "Email Address",
                    ]);
                }
                break;
            }
            case "password": {
                if (!value) {
                    validateMessage = MessageService.get("6004", ["Password"]);
                } else if (!validatePassword(value)) {
                    validateMessage = MessageService.get("6003", ["6", "30"]);
                }
                break;
            }
            case "confirmPassword": {
                if (value !== form.password.value) {
                    validateMessage = MessageService.get("6002", []);
                }
                break;
            }
        }

        setForm((prevForm: any) => ({
            ...prevForm,
            [item]: {
                validateMessage,
                value: value,
            },
        }));
    };

    const handleRegister = () => {
        for (const item in form) {
            handleOnChangeForm(item, (form as any)[item]?.value);
        }
        for (const item in form as any) {
            if (!!(form as any)[item].validateMessage) {
                return;
            }
        }
        fetch(`${env.getBackendHost()}/api/Users/register?useCookies=true`, {
            method: "POST",
            headers: {
                "Content-Type": "application/json",
            },
            body: JSON.stringify({
                username: form.username.value,
                email: form.email.value,
                password: form.password.value,
            }),
        }).then((x) => {
            if (x.ok) {
                setIsRegistered(true);
            } else {
                ToastService.showToast("8002", []);
            }
        });
    };

    const { test, returnPath } = props;
    return (
        <div className="flex h-screen justify-center">
            {isRegistered ? (
                <h1 className="w-100 pt-20" style={{ width: "40vw" }}>
                    Bạn đã đăng ký thành công. Chúng tôi đã gửi đường link xác
                    nhận vào tài khoản email của bạn. Vui lòng xác nhận.
                    <br></br>
                    <Link
                        className="text-[#429ff5]"
                        href="https://mail.google.com/mail/u/0/?hl=vi#inbox"
                    >
                        Nhấn để chuyển đến email của bạn
                    </Link>
                </h1>
            ) : (
                <form className="w-100 pt-20" style={{ width: "40vw" }}>
                    <h1 className="text-2xl">Tạo tài khoản mới</h1>
                    <div className="grid w-full items-center gap-1.5 pt-6">
                        <div className="flex items-center justify-between">
                            <Label htmlFor="username">Tên người dùng</Label>
                            <Label className="text-[#ff0000]">
                                {form.username.validateMessage}
                            </Label>
                        </div>
                        <Input
                            className="w-full"
                            type="text"
                            id="username"
                            placeholder="Tên người dùng"
                            value={form.username.value}
                            onChange={(e) =>
                                handleOnChangeForm("username", e.target.value)
                            }
                        />
                    </div>

                    <div className="grid w-full items-center gap-1.5 pt-6">
                        <div className="flex items-center justify-between">
                            <Label htmlFor="email">Địa chỉ Email</Label>
                            <Label className="text-[#ff0000]">
                                {form.email.validateMessage}
                            </Label>
                        </div>
                        <Input
                            className="w-full"
                            type="text"
                            id="email"
                            placeholder="Email"
                            value={form.email.value}
                            onChange={(e) =>
                                handleOnChangeForm("email", e.target.value)
                            }
                        />
                    </div>

                    <div className="grid w-full items-center gap-1.5 pt-6">
                        <div className="flex items-center justify-between">
                            <Label htmlFor="password">Mật khẩu</Label>
                            <Label className="text-[#ff0000]">
                                {form.password.validateMessage}
                            </Label>
                        </div>
                        <Input
                            className="w-full"
                            type="password"
                            id="password"
                            placeholder="Mật khẩu"
                            value={form.password.value}
                            onChange={(e) =>
                                handleOnChangeForm("password", e.target.value)
                            }
                        />
                    </div>

                    <div className="grid w-full items-center gap-1.5 pt-6">
                        <div className="flex items-center justify-between">
                            <Label htmlFor="confirmPassword">
                                Xác nhận mật khẩu
                            </Label>
                            <Label className="text-[#ff0000]">
                                {form.confirmPassword.validateMessage}
                            </Label>
                        </div>
                        <Input
                            className="w-full"
                            type="password"
                            id="confirmPassword"
                            placeholder="Xác nhận mật khẩu"
                            value={form.confirmPassword.value}
                            onChange={(e) =>
                                handleOnChangeForm(
                                    "confirmPassword",
                                    e.target.value,
                                )
                            }
                        />
                    </div>

                    <Button
                        type="button"
                        className="mt-6 w-full bg-[#00a3ff] text-white"
                        onClick={handleRegister}
                    >
                        Đăng ký
                    </Button>

                    <p className="py-8">
                        Đã có tài khoản?{" "}
                        <Link className="text-[#429ff5]" href="/login">
                            Đăng nhập
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
                            href={`${env.getBackendHost()}/api/Account/signin-google?returnUrl=${returnPath}`}
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
            )}
        </div>
    );
}

export interface AU_03_01ScreenProps {
    test: boolean;
    returnPath?: string;
}
