"use client";

import UserSettingLayout from "@/components/UserSettingLayout";
import React, { useEffect } from "react";
import { useState } from "react";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardFooter, CardHeader, CardTitle } from "@/components/ui/card";
import { Badge } from "@/components/ui/badge";
import { Input } from "@/components/ui/input";
import { Dot, Edit2, SendHorizontal, X } from "lucide-react";
import Image from "next/image";

import SiteLayout from "@/components/SiteLayout";
import { getSubscriptions, Subscription } from "@/utils/api/subscription";
import { toast } from "sonner";
import UwU from "@/components/UwU";
import Link from "next/link";
import cx from "classnames";
import { SUBSCRYPTION_STATUS } from "@/utils/consts";
import CancelSubscriptionButton from "../../_components/CancelSubscriptionButton";
import { subscribeDashiFanTier } from "@/utils/api/series";

export interface ManageSubscriptionScreenProps {
    seriesId: string;
}

export default function ManageSubscriptionScreen(props: ManageSubscriptionScreenProps) {
    const [subscriptions, setSubscriptions] = useState<Subscription[]>([]);
    const [titleFilter, setTitleFilter] = useState("");

    useEffect(() => {
        async function fetchSubscriptions() {
            const [data, err] = await getSubscriptions();
            if (err) {
                toast.error("Không thể tải, vui lòng thử lại!");
                return;
            }

            setSubscriptions(data);
        }
        fetchSubscriptions();
    }, []);
    console.log({ subscriptions });

    const filteredSubscriptions = subscriptions
        .filter((sub) => {
            const titleMatch = sub.series.title.toLowerCase().includes(titleFilter.toLowerCase());
            return titleMatch && (sub.status === 1 || sub.status === 2 || sub.status === 4);
        })
        .toReversed();

    const paginatedSubscriptions = filteredSubscriptions;
    console.log({ paginatedSubscriptions });
    return (
        <SiteLayout>
            <div className="container pt-6">
                <UserSettingLayout>
                    <div className="container mx-auto flex-grow space-y-6 p-4 text-neutral-50">
                        <h1 className="mb-6 text-2xl font-bold">Quản lý đăng ký DashiFan của bạn</h1>

                        <div className="mb-6 flex flex-wrap items-end justify-end gap-2">
                            <div>
                                <Input
                                    id="titleFilter"
                                    type="text"
                                    placeholder="Lọc theo tiêu đề"
                                    value={titleFilter}
                                    onChange={(e) => setTitleFilter(e.target.value)}
                                    className="border-neutral-700 bg-neutral-800 text-neutral-100"
                                />
                            </div>
                        </div>

                        {paginatedSubscriptions.map((sub) => (
                            <Card
                                key={sub.subscriptionId}
                                className="flex flex-col border-neutral-700 bg-neutral-800 lg:flex-row"
                            >
                                <div className="flex-shrink-0 p-4">
                                    <Image
                                        src={sub.series.thumbnailUrl}
                                        alt={sub.series.title}
                                        width={180}
                                        height={240}
                                        className="rounded-md object-cover"
                                    />
                                </div>
                                <CardContent className="flex-grow p-4">
                                    <CardHeader className="p-0">
                                        <div>
                                            <Badge
                                                variant={"destructive"}
                                                className={`hover:bg-${SUBSCRYPTION_STATUS[sub.status].bgColor} bd-${SUBSCRYPTION_STATUS[sub.status].bdColor} text-white bg-${SUBSCRYPTION_STATUS[sub.status].bgColor}`}
                                            >
                                                <span
                                                    className={`mr-2 h-2 w-2 rounded-full bg-${SUBSCRYPTION_STATUS[sub.status].bdColor} text-${SUBSCRYPTION_STATUS[sub.status].bdColor}`}
                                                ></span>
                                                {SUBSCRYPTION_STATUS[sub.status].content}
                                            </Badge>
                                        </div>
                                        <CardTitle className="text-xl font-semibold text-blue-300">
                                            {sub.series.title}
                                        </CardTitle>
                                        <p className="text-sm text-neutral-400">bởi {sub.series.author}</p>
                                    </CardHeader>
                                    <div className="mt-2 space-y-2">
                                        <p className="text-sm">
                                            Chương mới nhất: <UwU>No api :v</UwU>
                                            {/* {sub.latestChapter.number}{" "}
                                            {sub.latestChapter
                                                .isEarlyAccess && (
                                                <Badge
                                                    variant="secondary"
                                                    className="ml-2 bg-sky-700 text-sky-100"
                                                >
                                                    Truy cập sớm
                                                </Badge>
                                            )} */}
                                        </p>
                                        <p className="text-xs text-neutral-500">
                                            Cập nhật: {new Date(sub.series.lastModified).toLocaleDateString()}
                                        </p>
                                    </div>
                                    <div className="mt-4">
                                        <h3 className="font-semibold text-blue-300">Gói đăng ký hiện tại:</h3>
                                        <p>
                                            {sub.dashiFan.price.amount.toFixed(0)}
                                            đ/tháng
                                        </p>
                                        <ul className="mt-2 list-inside list-disc text-sm text-neutral-400">
                                            <li>Truy cập sớm tới {sub.dashiFan.perks} chương</li>
                                            <li>Mở khóa miễn phí cho các chương đã xuất bản</li>
                                        </ul>
                                    </div>
                                </CardContent>
                                <CardFooter className="flex flex-col space-y-2 p-4">
                                    {sub.status === 1 && (
                                        <Button
                                            variant="outline"
                                            className="w-full bg-sky-700 text-sky-100 hover:bg-blue-600"
                                            onClick={async () => {
                                                const url = new URL(window.location.href);
                                                url.pathname = `/series/${sub.series.id}`;
                                                url.searchParams.delete("successPayment");
                                                url.searchParams.delete("tierId");
                                                url.searchParams.delete("ba_token");
                                                url.searchParams.delete("subscription_id");
                                                url.searchParams.delete("token");

                                                url.searchParams.set("successPayment", "true");
                                                url.searchParams.set("tierId", sub.dashiFan.id);
                                                url.searchParams.set("tab", "dashifan");
                                                const [paymentUrl, err] = await subscribeDashiFanTier({
                                                    seriesId: sub.series.id,
                                                    tierId: sub.dashiFan.id,
                                                    returnUrl: url.toString(),
                                                    cancelUrl: window.location.href,
                                                });

                                                if (err) {
                                                    toast.error("Không thể đăng ký, vui lòng thử lại sau!");
                                                    return;
                                                }

                                                window.location.href = paymentUrl;
                                            }}
                                        >
                                            <SendHorizontal className="mr-2 h-4 w-4" /> Tiếp tục thanh toán
                                        </Button>
                                    )}

                                    {sub.status === 2 && (
                                        <Button
                                            asChild
                                            variant="outline"
                                            className="w-full bg-sky-700 text-sky-100 hover:bg-blue-600"
                                        >
                                            <Link href={`/series/${sub.series.id}?tab=dashifan`}>
                                                <Edit2 className="mr-2 h-4 w-4" /> Thay đổi gói
                                            </Link>
                                        </Button>
                                    )}

                                    {sub.status === 2 && <CancelSubscriptionButton subId={sub.subscriptionId} />}
                                    <p className="text-xs text-neutral-500">
                                        Đã đăng ký từ: {new Date(sub.subscribedSince).toLocaleDateString()}
                                    </p>
                                </CardFooter>
                            </Card>
                        ))}
                        {paginatedSubscriptions.length === 0 && (
                            <div className="text-center text-neutral-400">Không tìm thấy kết quả phù hợp</div>
                        )}
                    </div>
                </UserSettingLayout>
            </div>
        </SiteLayout>
    );
}
