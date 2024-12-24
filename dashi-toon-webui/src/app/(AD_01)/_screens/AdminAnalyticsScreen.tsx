"use client";
import AdminSettingLayout from "@/components/AdminSettingLayout";
import SiteLayout from "@/components/SiteLayout";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { ROLES } from "@/utils/consts";
import React from "react";
import cx from "classnames";
import { ExternalLink } from "lucide-react";
import Image from "next/image";
import { Button } from "@/components/ui/button";
import Link from "next/link";
import * as env from "@/utils/env";

export default function AdminAnalyticsScreen() {
    return (
        <SiteLayout allowedRoles={[ROLES.Administrator]} hiddenUntilLoaded>
            <div className="container pt-6">
                <AdminSettingLayout>
                    <div className="p-6 text-neutral-100">
                        <h1 className="text-3xl font-bold">Thống kê hệ thống</h1>
                        <p className="mb-6 mt-0 text-muted-foreground">
                            Thống kê hệ thống giúp bạn hiểu rõ hơn về hoạt động của hệ thống DashiToon.
                        </p>

                        <Card className={cx("font-bold, bg-gradient-to-tr from-transparent to-amber-600/30 text-2xl")}>
                            <CardHeader className="relative">
                                <CardTitle className="flex flex-wrap md:flex-nowrap">
                                    <div>
                                        <h2 className="mt-7">
                                            Báo cáo phân tích hệ thống của bạn đã sẵn sàng trên Google Analytics!
                                        </h2>
                                        <p className="text-sm font-normal text-muted-foreground">
                                            Để xem báo cáo chi tiết, vui lòng truy cập Google Analytics của bạn bằng nút
                                            bên dưới.
                                        </p>
                                    </div>
                                    <Image src="/images/GA4_Logo.png" alt="google analytics" width={250} height={200} />
                                </CardTitle>
                            </CardHeader>
                            <CardContent className="flex justify-start pt-4 text-center text-3xl">
                                <Button>
                                    <Link
                                        href={`https://analytics.google.com/analytics/web/#/${env.GA_PROP_ID ?? "p468047885"}/reports/intelligenthome`}
                                        target="_blank"
                                        className="flex items-center gap-2"
                                    >
                                        <ExternalLink />
                                        Truy cập Google Analytics
                                    </Link>
                                </Button>
                            </CardContent>
                        </Card>
                    </div>
                </AdminSettingLayout>
            </div>
        </SiteLayout>
    );
}
