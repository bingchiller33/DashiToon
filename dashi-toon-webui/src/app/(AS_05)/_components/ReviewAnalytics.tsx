import React from "react";
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card";
import {
    Analytics,
    getViewAnalytics,
    getViewRanking,
    ViewAnalyticsResp,
    RankingResp,
    ReviewAnalyticsResp,
    getReviewAnalytics,
} from "@/utils/api/analytics";
import { ChartNoAxesColumn, Eye, Hash, MessageSquareText, Star } from "lucide-react";
import Insight from "../_components/Insight";
import { OmniChart } from "../_components/OmniChart";
import { ReviewChart } from "../_components/ReviewChart";
import { ViewCountByWeek } from "../_components/ViewCountByWeek";
import { useEffect, useState } from "react";
import { DateRange } from "react-day-picker";
import { toast } from "sonner";
import ViewAnalytics from "./ViewAnalytics";
import CommentAnalytics from "./CommentAnalytics";
import { Skeleton } from "@/components/ui/skeleton";
import { formatDateRange } from "@/utils";

export interface ReviewAnalyticsProps {
    seriesId: string;
    current: DateRange;
    compare?: DateRange;
}

export default function ReviewAnalytics(props: ReviewAnalyticsProps) {
    const { seriesId, current, compare } = props;
    const [isLoading, setisLoading] = useState<boolean>(true);
    const [data, setData] = useState<ReviewAnalyticsResp>();

    const currentStr = formatDateRange(current);
    const compareStr = compare ? formatDateRange(compare) : "N/A";

    useEffect(() => {
        async function fetchData() {
            const [data, err] = await getReviewAnalytics({
                current,
                compare,
                seriesId,
            });
            if (err) {
                toast.error("Có lỗi xảy ra khi tải dữ liệu đánh giá. Vui lòng thử lại sau");
                return;
            }
            setData(data);
            setisLoading(false);
        }
        Promise.all([fetchData()]);
    }, [current, compare, seriesId]);

    return (
        <div className="mt-2 flex gap-2">
            <Card className="w-full">
                <CardHeader>
                    <CardTitle className="flex gap-2">
                        <div className="flex gap-2">
                            <Star /> Thống kê đánh giá
                        </div>
                    </CardTitle>
                    <CardDescription>Thống kê chi tiết về đánh giá của bộ truyện</CardDescription>
                </CardHeader>
                <CardContent className="gap-2 space-y-4 md:flex">
                    <div className="flex-grow">
                        {data ? (
                            <ReviewChart current={currentStr} compare={compareStr} data={data.data} />
                        ) : (
                            <Skeleton className="h-[350px] w-full" />
                        )}
                    </div>
                    <div>
                        <Insight
                            icon={<Hash className="h-6 w-6 text-blue-400" />}
                            isLoading={isLoading}
                            title="Tổng cộng toàn thời gian"
                            current={data?.total ?? 0}
                            compare={data?.totalCompare ?? 0}
                        />
                        <Insight
                            icon={<ChartNoAxesColumn className="h-6 w-6 text-green-400" />}
                            isLoading={isLoading}
                            title="Thứ hạng trên hệ thống"
                            current={data?.ranking ?? 0}
                            compare={data?.rankingCompare ?? 0}
                            prefix="#"
                        />
                        <Insight
                            icon={<Star className="h-6 w-6 text-orange-400" />}
                            isLoading={isLoading}
                            title="Tỉ lệ bình luận tích cực"
                            current={data?.positive ?? 0}
                            compare={data?.positiveCompare ?? 0}
                            formatter={(v) => v.toFixed(1) + "%"}
                        />
                    </div>
                </CardContent>
            </Card>
        </div>
    );
}
