import { formatDateRange } from "@/utils";
import {
    KanaAnalyticsResp,
    getKanaAnalytics,
    getKanaRanking,
    RankingResp,
} from "@/utils/api/analytics";
import { Coins, MessageSquareText } from "lucide-react";
import { useEffect, useState } from "react";
import { DateRange } from "react-day-picker";
import { toast } from "sonner";
import { OmniChart } from "./OmniChart";

export interface KanaAnalyticsProps {
    seriesId: string;
    current: DateRange;
    compare?: DateRange;
}

export default function KanaAnalytics(props: KanaAnalyticsProps) {
    const { seriesId, current, compare } = props;
    const [kanaRanking, setKanaRanking] = useState<RankingResp>();
    const [kanaAnalytics, setKanaAnalytics] = useState<KanaAnalyticsResp>();
    const [kanaSearch, setKanaSearch] = useState<string>("");
    const [kanaPage, setKanaPage] = useState<number>(1);
    const [kanaDesc, setKanaDesc] = useState<boolean>(false);

    const currentStr = formatDateRange(current);
    const compareStr = compare ? formatDateRange(compare) : "N/A";

    useEffect(() => {
        async function fetchKana() {
            const [data, err] = await getKanaAnalytics({
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
            setKanaAnalytics(data);
        }
        Promise.all([fetchKana()]);
    }, [current, compare, seriesId]);

    useEffect(() => {
        async function fetchKanaRanking() {
            const [data, err] = await getKanaRanking({
                current,
                compare,
                seriesId,
                desc: kanaDesc,
                pageNumber: kanaPage,
                query: kanaSearch,
            });
            if (err) {
                toast.error(
                    "Có lỗi xảy ra khi tải dữ liệu lượt xem. Vui lòng thử lại sau",
                );
                return;
            }
            setKanaRanking(data);
        }

        fetchKanaRanking();
    }, [kanaPage, kanaSearch, kanaDesc, seriesId, current, compare]);

    return (
        <div className="mt-2 grid gap-2 2xl:grid-cols-2">
            <OmniChart
                data={kanaAnalytics!}
                ranking={kanaRanking?.items}
                formatter={(e) => e.toString()}
                titleGeneral={
                    <>
                        <Coins />
                        KanaCoin - Tổng quan
                    </>
                }
                descGeneral="Thống kê tổng quan KanaCoin kiếm được từ tác phẩm trong thời gian qua"
                titleRanking={
                    <>
                        <Coins />
                        KanaCoin - So sánh
                    </>
                }
                descRanking="Thống kê so sánh KanaCoin kiếm được từ các chương truyện trong tác phẩm trong thời gian qua"
                current={currentStr}
                compare={compareStr}
                onPageChange={(page) => setKanaPage(page)}
                onSearch={(query) => setKanaSearch(query)}
                onSort={() => setKanaDesc(!kanaDesc)}
                rankingCurrentPage={kanaRanking?.pageNumber ?? 1}
                rankingTotalPages={kanaRanking?.totalPages ?? 10}
                search={kanaSearch}
            />
        </div>
    );
}
