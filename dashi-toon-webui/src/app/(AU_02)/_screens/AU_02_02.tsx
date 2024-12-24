"use client";

import React, { useEffect } from "react";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { validateEmail } from "@/utils";
import MessageService from "@/utils/service/MessageService";
import ToastService from "@/utils/service/ToastService";
import * as env from "@/utils/env";
import { useSearchParams } from "next/navigation";
import { Toast } from "@radix-ui/react-toast";

interface ValidationError {
    field: string;
    error: string | null;
}
const validateField = (
    fieldName: string,
    value: string,
    additionalValue?: string,
): ValidationError => {
    switch (fieldName) {
        case "password":
            if (!value) {
                return {
                    field: fieldName,
                    error: MessageService.get("6004", ["Password"]),
                };
            } else if (value.length < 6 || value.length > 30) {
                return {
                    field: fieldName,
                    error: MessageService.get("6003", ["6", "30"]),
                };
            } else if (!/[^\w\s]/.test(value)) {
                return {
                    field: fieldName,
                    error: "Must have at least one non-alphanumeric character.",
                };
            } else if (!/[A-Z]/.test(value)) {
                return {
                    field: fieldName,
                    error: "Must have at least one uppercase ('A'-'Z').",
                };
            }
            break;

        case "confirmPassword":
            if (!value) {
                return {
                    field: fieldName,
                    error: MessageService.get("6004", ["Confirm Password"]),
                };
            } else if (value !== additionalValue) {
                return {
                    field: fieldName,
                    error: MessageService.get("6002", []),
                };
            }
            break;
        default:
            return { field: fieldName, error: null };
    }
    return { field: fieldName, error: null };
};

export default function AU_02_02Screen(props: AU_02_02ScreenProps) {
    const searchParams = useSearchParams();
    const emailFromQuery = searchParams.get("email");
    const resetCodeFromQuery = searchParams.get("resetCode");

    const [resetCode, setResetCode] = React.useState<string>("");
    const [email, setEmail] = React.useState<string>("");
    const [password, setPassword] = React.useState<string>("");
    const [confirmPassword, setConfirmPassword] = React.useState<string>("");
    const [errorMsgs, setErrorMsgs] = React.useState<Record<string, string>>(
        {},
    );

    useEffect(() => {
        if (emailFromQuery) {
            setEmail(emailFromQuery);
        }
        if (resetCodeFromQuery) {
            setResetCode(resetCodeFromQuery);
        }
    }, [emailFromQuery, resetCodeFromQuery]);

    const handleFieldBlur = (
        fieldName: string,
        value: string,
        additionalValue?: string,
    ) => {
        const { error } = validateField(fieldName, value, additionalValue);
        if (error) {
            setErrorMsgs((prevErrors) => ({
                ...prevErrors,
                [fieldName]: error,
            }));
        } else {
            setErrorMsgs((prevErrors) => ({
                ...prevErrors,
                [fieldName]: "",
            }));
        }
    };

    const handleResetPassword = async (e: React.FormEvent<HTMLFormElement>) => {
        e.preventDefault();

        const emailError = validateField("email", email).error;
        const passwordError = validateField("password", password).error;
        const confirmPasswordError = validateField(
            "confirmPassword",
            confirmPassword,
            password,
        ).error;
        const resetCodeError = validateField("resetCode", resetCode).error;

        if (
            emailError ||
            passwordError ||
            confirmPasswordError ||
            resetCodeError
        ) {
            setErrorMsgs({
                email: emailError || "",
                password: passwordError || "",
                confirmPassword: confirmPasswordError || "",
                resetCode: resetCodeError || "",
            });
            ToastService.showToast("6001", ["fields in form"]);
            return;
        }

        try {
            const response = await fetch(
                `${env.getBackendHost()}/api/Users/resetPassword`,
                {
                    method: "POST",
                    headers: {
                        "Content-Type": "application/json",
                    },
                    body: JSON.stringify({
                        resetCode,
                        email,
                        newPassword: password,
                    }),
                },
            );
            console.log(response);

            if (response.ok) {
                window.location.href = "/login";
                ToastService.showToast("4001", ["password changed!"]);
            } else {
                ToastService.showToast("8002", []);
            }
        } catch (err) {
            ToastService.showToast("8002", []);
        }
    };

    return (
        <div className="flex h-screen justify-center">
            <div className="w-80 pt-40 lg:w-96">
                <h1 className="text-2xl">Đặt Lại Mật Khẩu</h1>
                <form onSubmit={handleResetPassword}>
                    <div className="mt-6">
                        <div className="mb-4">
                            <Label htmlFor="password" className="text-white">
                                Mật khẩu
                            </Label>
                            <Input
                                type="password"
                                name="password"
                                placeholder="Mật khẩu"
                                className="mt-2 w-full"
                                onBlur={(e) =>
                                    handleFieldBlur("password", e.target.value)
                                }
                                onChange={(e) => setPassword(e.target.value)}
                                value={password}
                            />
                            {errorMsgs.password && (
                                <span className="mt-2 block text-xs text-red-500">
                                    {errorMsgs.password}
                                </span>
                            )}
                        </div>

                        <div className="mb-4">
                            <Label
                                htmlFor="confirmPassword"
                                className="text-white"
                            >
                                Xác nhận mật khẩu
                            </Label>
                            <Input
                                type="password"
                                name="confirmPassword"
                                placeholder="Xác nhận mật khẩu"
                                className="mt-2 w-full"
                                onBlur={(e) =>
                                    handleFieldBlur(
                                        "confirmPassword",
                                        e.target.value,
                                        password,
                                    )
                                }
                                onChange={(e) =>
                                    setConfirmPassword(e.target.value)
                                }
                                value={confirmPassword}
                            />
                            {errorMsgs.confirmPassword && (
                                <span className="mt-2 block text-xs text-red-500">
                                    {errorMsgs.confirmPassword}
                                </span>
                            )}
                        </div>
                    </div>
                    <Button
                        type="submit"
                        className="mt-6 w-full bg-[#00a3ff] text-white hover:bg-[#00a3ff]/80"
                    >
                        Đặt Lại Mật Khẩu
                    </Button>
                </form>
            </div>
        </div>
    );
}

export interface AU_02_02ScreenProps {
    test: boolean;
    returnPath?: string;
}
