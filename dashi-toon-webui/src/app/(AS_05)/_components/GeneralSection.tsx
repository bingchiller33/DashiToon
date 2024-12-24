"use client";
import React, { ReactNode, useEffect, useState } from "react";
import { CircleDollarSign, Eye, Star, Zap } from "lucide-react";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import StatNum from "../_components/StatNum";
import cx from "classnames";
import { Separator } from "@/components/ui/separator";
import { GeneralAnalyticsResp, getGeneralAnalytics } from "@/utils/api/analytics";
import { DateRange } from "react-day-picker";
import { toast } from "sonner";

export interface GeneralSection {
    seriesId: string;
    current: DateRange;
    compare?: DateRange;
}

export default function GeneralSection(props: GeneralSection) {
    const { seriesId, current, compare } = props;
    const [isLoading, setIsLoading] = useState(true);
    const [data, setData] = useState<GeneralAnalyticsResp>();

    useEffect(() => {
        async function fetchData() {
            setIsLoading(true);
            const [data, err] = await getGeneralAnalytics({
                current,
                compare,
                seriesId,
            });

            if (err) {
                toast.error("Có lỗi xảy ra khi tải dữ liệu tổng quan. Vui lòng thử lại sau");
                return;
            }

            setData(data);
            setIsLoading(false);
        }

        fetchData();
    }, [current, compare, seriesId]);

    const aView = data?.currentView ?? 0;
    const bView = data?.compareView ?? 0;
    const dView = aView - bView;

    const aRating = data?.currentRating ?? 0;
    const bRating = data?.compareRating ?? 0;
    const dRating = aRating - bRating;

    const aRevenue = data?.currentRevenue ?? 0;
    const bRevenue = data?.compareRevenue ?? 0;
    const dRevenue = aRevenue - bRevenue;

    const aDashiFan = data?.currentDashiFan ?? 0;
    const bDashiFan = data?.compareDashiFan ?? 0;
    const dDashiFan = aDashiFan - bDashiFan;

    function fmtNum(num: number) {
        return Intl.NumberFormat("vi-VN").format(num);
    }

    return (
        <section className="container">
            <h2 className="text-2xl">Tổng quan</h2>
            <p className="mb-4 text-muted-foreground">Thông tin cơ bản về bộ truyện của bạn</p>
            <div className="grid gap-2 md:grid-cols-2 xl:grid-cols-4">
                <GeneralCard
                    current={fmtNum(aView)}
                    compare={compare ? fmtNum(dView) : undefined}
                    icon={<Eye className="h-10 w-10 text-blue-400" />}
                    title="Tổng số lượt đọc"
                    bg="to-blue-400/15"
                    isLoading={isLoading}
                    isGood={dView >= 0}
                />
                <GeneralCard
                    current={fmtNum(aRating)}
                    compare={compare ? fmtNum(dRating) : undefined}
                    icon={<Star className="h-10 w-10 text-orange-400" />}
                    title="Mức độ đánh giá"
                    bg="to-orange-400/15"
                    isGood={dRating >= 0}
                    isLoading={isLoading}
                />

                <GeneralCard
                    current={fmtNum(aRevenue)}
                    compare={compare ? fmtNum(dRevenue) : undefined}
                    icon={<CircleDollarSign className="h-10 w-10 text-green-400" />}
                    title="Tổng thu nhập"
                    bg="to-green-400/15"
                    isLoading={isLoading}
                    isGood={dRevenue >= 0}
                />
                <GeneralCard
                    current={fmtNum(aDashiFan)}
                    compare={compare ? fmtNum(dDashiFan) : undefined}
                    icon={<Zap className="h-10 w-10 text-blue-400" />}
                    title="Tổng số DashiFan"
                    bg="to-blue-400/15"
                    isLoading={isLoading}
                    isGood={dDashiFan >= 0}
                />
            </div>
            <Separator className="my-4 border-t-2" />
        </section>
    );
}

export interface GeneralCardProps {
    title: string;
    current: ReactNode;
    compare: ReactNode;
    icon: ReactNode;
    bg?: string;
    isLoading?: boolean;
    isGood: boolean;
}

export function GeneralCard(props: GeneralCardProps) {
    const { title, current, compare, icon, isLoading, isGood } = props;
    return (
        <Card className={cx("bg-gradient-to-tr from-transparent text-2xl font-bold", props.bg)}>
            <CardHeader className="relative">
                <CardTitle>
                    {title}
                    <div className="absolute right-4 top-4">{icon}</div>
                </CardTitle>
            </CardHeader>
            <CardContent className="flex justify-start pt-4 text-center text-3xl">
                <StatNum current={current} compare={compare} isGood={isGood} isLoading={isLoading} />
            </CardContent>
        </Card>
    );
}
