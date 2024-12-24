"use client";

import { Button } from "@/components/ui/button";
import { Checkbox } from "@/components/ui/checkbox";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { setUsernameAndLogin } from "@/utils/api/user";
import React, { useState } from "react";
import { toast } from "sonner";

export default function SetUsernamePage(props: SetUsernamePageProps) {
    const { returnUrl, token } = props;
    const [username, setUsername] = useState("");
    const [tos, setTos] = useState(false);

    async function handleSU() {
        if (!tos) {
            toast.error("Vui lòng đồng ý với Chính sách bảo mật và Điều khoản sử dụng trước khi đăng ký.");
            return;
        }

        const reg = /^[a-zA-Z]+$/g;
        if (!reg.test(username)) {
            toast.error("Tên người dùng chỉ chứa ký tự chữ cái không dấu.");
            return;
        }

        const [data, err] = await setUsernameAndLogin(token, username, returnUrl ?? "/");
        if (err) {
            toast.error("Đã xảy ra lỗi khi đăng ký tên người dùng. Vui lòng thử lại sau.");
            return;
        }
    }

    return (
        <div className="flex h-screen justify-center">
            <form className="w-96 pt-40 sm:w-96">
                <h1 className="text-2xl">Đăng ký qua Google </h1>
                <p className="text-muted-foreground">
                    Bạn đã xác thực thành công với Google. Vui lòng nhập tên người dùng và nhấn nút Đăng ký để hoàn tất.
                </p>
                <div className="grid w-full items-center gap-1.5 pt-6">
                    <Label htmlFor="email">Tên người dùng mới</Label>
                    <Input
                        className="w-full"
                        type="email"
                        id="email"
                        placeholder="Tên người dùng mới"
                        required
                        value={username}
                        onChange={(e) => setUsername(e.target.value)}
                    />
                    <p className="text-muted-foreground">Bạn không thể thay đổi tên người dùng sau khi đã đăng ký.</p>
                </div>

                <div className="mt-4 flex items-center space-x-2">
                    <Checkbox id="terms" checked={tos} onCheckedChange={(e) => setTos(e as boolean)} />
                    <Label htmlFor="terms" className="text-sm">
                        Tôi đã đọc và đồng ý với{" "}
                        <a href="/privacy" className="text-blue-500 hover:underline">
                            Chính sách bảo mật
                        </a>{" "}
                        và{" "}
                        <a href="/terms" className="text-blue-500 hover:underline">
                            Điều khoản sử dụng
                        </a>
                    </Label>
                </div>

                <Button className="mt-6 w-full bg-[#00a3ff] text-white" onClick={handleSU} type="button">
                    Đăng ký
                </Button>
            </form>
        </div>
    );
}

export interface SetUsernamePageProps {
    returnUrl?: string;
    token: string;
}
