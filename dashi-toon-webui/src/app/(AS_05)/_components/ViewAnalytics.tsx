import {
    Card,
    CardContent,
    CardDescription,
    CardHeader,
    CardTitle,
} from "@/components/ui/card";
import {
    Analytics,
    getViewAnalytics,
    getViewRanking,
    ViewAnalyticsResp,
    RankingResp,
} from "@/utils/api/analytics";
import {
    ChartNoAxesColumn,
    Eye,
    Hash,
    MessageSquareText,
    Star,
} from "lucide-react";
import Insight from "../_components/Insight";
import { OmniChart } from "../_components/OmniChart";
import { ReviewChart } from "../_components/ReviewChart";
import { ViewCountByWeek } from "../_components/ViewCountByWeek";
import { useEffect, useState } from "react";
import { DateRange } from "react-day-picker";
import { toast } from "sonner";
import { formatDateRange } from "@/utils";

export interface ViewAnalyticsProps {
    seriesId: string;
    current: DateRange;
    compare?: DateRange;
}

export default function ViewAnalytics(props: ViewAnalyticsProps) {
    const { seriesId, current, compare } = props;
    const [viewRanking, setViewRanking] = useState<RankingResp>();
    const [viewAnalytics, setViewAnalytics] = useState<ViewAnalyticsResp>();
    const [viewSearch, setViewSearch] = useState<string>("");
    const [viewPage, setViewPage] = useState<number>(1);
    const [viewDesc, setViewDesc] = useState<boolean>(false);

    const currentStr = formatDateRange(current);
    const compareStr = compare ? formatDateRange(compare) : "N/A";

    useEffect(() => {
        async function fetchView() {
            const [data, err] = await getViewAnalytics({
                current,
                compare,
                seriesId,
            });
            if (err) {
                toast.error(
                    "Có lỗi xảy ra khi tải dữ liệu lượt xem. Vui lòng thử lại sau",
                );
                return;
            }
            setViewAnalytics(data);
        }
        Promise.all([fetchView()]);
    }, [current, compare, seriesId]);

    useEffect(() => {
        async function fetchViewRanking() {
            const [data, err] = await getViewRanking({
                current,
                compare,
                seriesId,
                desc: viewDesc,
                pageNumber: viewPage,
                query: viewSearch,
            });
            if (err) {
                toast.error(
                    "Có lỗi xảy ra khi tải dữ liệu lượt xem. Vui lòng thử lại sau",
                );
                return;
            }
            setViewRanking(data);
        }

        fetchViewRanking();
    }, [viewPage, viewSearch, viewDesc, seriesId, current, compare]);

    return (
        <div className="flex flex-wrap gap-2">
            <ViewCountByWeek
                current={currentStr}
                compare={compareStr}
                data={viewAnalytics}
            />

            <OmniChart
                data={viewAnalytics!}
                ranking={viewRanking?.items}
                formatter={(e) => e.toString()}
                titleGeneral={
                    <>
                        <Eye />
                        Lượt xem - Tổng quan
                    </>
                }
                descGeneral="Thống kê tổng quan lượt xem tác phẩm trong thời gian qua"
                titleRanking={
                    <>
                        <Eye />
                        Lượt xem - So sánh
                    </>
                }
                descRanking="Thống kê so sánh lượt xem của các chương truyện trong tác phẩm trong thời gian qua"
                current={currentStr}
                compare={compareStr}
                onPageChange={(page) => setViewPage(page)}
                onSearch={(query) => setViewSearch(query)}
                onSort={() => setViewDesc(!viewDesc)}
                rankingCurrentPage={viewRanking?.pageNumber ?? 1}
                rankingTotalPages={viewRanking?.totalPages ?? 10}
                search={viewSearch}
            />
        </div>
    );
}
