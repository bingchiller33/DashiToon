import { formatDateRange } from "@/utils";
import {
    DashiFanAnalyticsResp,
    getDashiFanAnalytics,
    getDashiFanRanking,
    RankingResp,
    TierRanking,
} from "@/utils/api/analytics";
import { Zap } from "lucide-react";
import { useEffect, useState } from "react";
import { DateRange } from "react-day-picker";
import { toast } from "sonner";
import { DashiFanChart } from "./DashiFanChart";

export interface DashiFanAnalyticsProps {
    seriesId: string;
    current: DateRange;
    compare?: DateRange;
}

export default function DashiFanAnalytics(props: DashiFanAnalyticsProps) {
    const { seriesId, current, compare } = props;
    const [dashiFanRanking, setDashiFanRanking] = useState<RankingResp<TierRanking>>();
    const [dashiFanAnalytics, setDashiFanAnalytics] = useState<DashiFanAnalyticsResp>();
    const [dashiFanSearch, setDashiFanSearch] = useState<string>("");
    const [dashiFanPage, setDashiFanPage] = useState<number>(1);
    const [dashiFanDesc, setDashiFanDesc] = useState<boolean>(false);

    const currentStr = formatDateRange(current);
    const compareStr = compare ? formatDateRange(compare) : "N/A";

    useEffect(() => {
        async function fetchDashiFan() {
            const [data, err] = await getDashiFanAnalytics({
                current,
                compare,
                seriesId,
            });
            if (err) {
                toast.error("Có lỗi xảy ra khi tải dữ liệu lượt xem. Vui lòng thử lại sau");
                return;
            }
            setDashiFanAnalytics(data);
        }
        Promise.all([fetchDashiFan()]);
    }, [current, compare, seriesId]);

    useEffect(() => {
        async function fetchDashiFanRanking() {
            const [data, err] = await getDashiFanRanking({
                current,
                compare,
                seriesId,
                desc: dashiFanDesc,
                pageNumber: dashiFanPage,
                query: dashiFanSearch,
            });
            if (err) {
                toast.error("Có lỗi xảy ra khi tải dữ liệu lượt xem. Vui lòng thử lại sau");
                return;
            }
            setDashiFanRanking(data);
        }

        fetchDashiFanRanking();
    }, [dashiFanPage, dashiFanSearch, dashiFanDesc, seriesId, current, compare]);

    const data = dashiFanAnalytics ? { ...dashiFanAnalytics, topChapters: dashiFanAnalytics.topTiers } : undefined;

    return (
        <div className="mt-2 grid gap-2 2xl:grid-cols-2">
            <DashiFanChart
                data={data}
                ranking={dashiFanRanking?.items}
                formatter={(e) => e.toString()}
                titleGeneral={
                    <>
                        <Zap />
                        DashiFan - Tổng quan
                    </>
                }
                descGeneral="Thống kê tổng quan DashiFan của tác phẩm trong thời gian qua"
                titleRanking={
                    <>
                        <Zap />
                        DashiFan - So sánh
                    </>
                }
                descRanking="Thống kê so sánh DashiFan của các hạng trong tác phẩm trong thời gian qua"
                current={currentStr}
                compare={compareStr}
                onPageChange={(page) => setDashiFanPage(page)}
                onSearch={(query) => setDashiFanSearch(query)}
                onSort={() => setDashiFanDesc(!dashiFanDesc)}
                rankingCurrentPage={dashiFanRanking?.pageNumber ?? 1}
                rankingTotalPages={dashiFanRanking?.totalPages ?? 10}
                search={dashiFanSearch}
            />
        </div>
    );
}
