"use client";

import { useEffect, useState } from "react";
import { DailyCheckIn } from "../components/DailyCheckIn";
import { MissionRewards } from "../components/MissionReward";
import { TransactionHistory } from "../components/TransactionHistory";
import { BuyKanaGold } from "../components/BuyKanaGold";
import SiteLayout from "@/components/SiteLayout";
import {
    Card,
    CardContent,
    CardDescription,
    CardHeader,
    CardTitle,
} from "@/components/ui/card";
import { Coins } from "lucide-react";
import { toast } from "sonner";
import {
    getUserKanas,
    getUserMetadata,
    UserKanas,
    UserMetadata,
} from "@/utils/api/reader/kana";
import { LoadingSpinner } from "@/components/ui/spinner";

export default function ManageCurrency() {
    const [userKanas, setUserKanas] = useState<UserKanas | null>(null);
    const [userMetadata, setUserMetadata] = useState<UserMetadata | null>(null);
    const [resetTime, setResetTime] = useState("");
    const [isLoading, setIsLoading] = useState(true);

    useEffect(() => {
        const fetchData = async () => {
            setIsLoading(true);
            await Promise.all([fetchUserKanas(), fetchUserMetadata()]);
            setIsLoading(false);
        };
        fetchData();
    }, []);

    const fetchUserKanas = async () => {
        const [data, error] = await getUserKanas();
        if (data) {
            setUserKanas(data);
        } else {
            toast.error("Lỗi khi tải thông tin Kana", error);
        }
    };

    const fetchUserMetadata = async () => {
        const [data, error] = await getUserMetadata();
        if (data) {
            setUserMetadata(data);
        } else {
            toast.error("Lỗi khi tải thông tin nhiệm vụ", error);
        }
    };

    useEffect(() => {
        const updateResetTime = () => {
            const now = new Date();
            const tomorrow = new Date(
                now.getFullYear(),
                now.getMonth(),
                now.getDate() + 1,
            );
            const timeUntilReset = tomorrow.getTime() - now.getTime();
            const hours = Math.floor(timeUntilReset / (1000 * 60 * 60));
            const minutes = Math.floor(
                (timeUntilReset % (1000 * 60 * 60)) / (1000 * 60),
            );
            setResetTime(`${hours}:${minutes.toString().padStart(2, "0")}`);
        };

        updateResetTime();
        const interval = setInterval(updateResetTime, 60000);
        return () => clearInterval(interval);
    }, []);

    const getKanaAmount = (kanaType: 1 | 2) => {
        return (
            userKanas?.totals.find((total) => total.kanaType === kanaType)
                ?.amount ?? 0
        );
    };

    if (isLoading) {
        return (
            <SiteLayout>
                <div className="flex h-screen items-center justify-center">
                    <LoadingSpinner
                        size={64}
                        className="text-blue-500 opacity-75 transition-opacity ease-in-out"
                    />
                </div>
            </SiteLayout>
        );
    }

    return (
        <SiteLayout>
            <div className="mx-auto min-h-screen max-w-6xl text-gray-200">
                <div className="container mx-auto space-y-8 p-4">
                    <div className="text-center">
                        <h1 className="mb-4 text-3xl font-bold">Quản Lý Xu</h1>
                        <div className="flex items-center justify-center space-x-8">
                            <div>
                                <span className="flex text-2xl font-bold text-yellow-400">
                                    <Coins className="mr-3 h-8 w-8 flex-shrink-0 text-gray-400" />
                                    {getKanaAmount(2).toLocaleString() ?? 0}
                                </span>
                            </div>
                            <div>
                                <span className="flex text-2xl font-bold text-yellow-500">
                                    <Coins className="mr-3 h-8 w-8 flex-shrink-0 text-yellow-400" />
                                    {getKanaAmount(1).toLocaleString() ?? 0}
                                </span>
                            </div>
                        </div>
                    </div>
                    <div className="grid grid-cols-1 gap-8 md:grid-cols-2">
                        <Card className="border-none bg-neutral-900 shadow-lg shadow-black/50 backdrop-blur-sm">
                            <CardHeader className="border-b border-neutral-700 pb-4">
                                <CardTitle className="flex items-center justify-between">
                                    <div className="flex items-center text-2xl font-bold">
                                        <Coins className="mr-3 h-8 w-8 flex-shrink-0 text-gray-400" />
                                        Kana Coin
                                    </div>
                                    <p className="mb-4 text-sm text-gray-400">
                                        Đặt lại sau {resetTime} giờ
                                    </p>
                                </CardTitle>
                                <CardDescription>
                                    Nhận Kana Coin hàng ngày miễn phí.
                                </CardDescription>
                            </CardHeader>
                            <CardContent className="divide-y divide-neutral-700 p-0">
                                <div className="p-6">
                                    <DailyCheckIn
                                        isCheckedIn={
                                            userMetadata?.isCheckedIn ?? false
                                        }
                                        onCheckIn={() => {
                                            fetchUserKanas();
                                            fetchUserMetadata();
                                        }}
                                    />
                                </div>
                                <div className="p-6">
                                    <MissionRewards
                                        onClaimReward={() => {
                                            fetchUserKanas();
                                            fetchUserMetadata();
                                        }}
                                    />
                                </div>
                            </CardContent>
                        </Card>
                        <BuyKanaGold />
                    </div>
                    <TransactionHistory />
                </div>
            </div>
        </SiteLayout>
    );
}
