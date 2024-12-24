import { formatDateRange } from "@/utils";
import {
    CommentAnalyticsResp,
    getCommentAnalytics,
    getCommentRanking,
    RankingResp,
} from "@/utils/api/analytics";
import { MessageSquareText } from "lucide-react";
import { useEffect, useState } from "react";
import { DateRange } from "react-day-picker";
import { toast } from "sonner";
import { OmniChart } from "./OmniChart";

export interface CommentAnalyticsProps {
    seriesId: string;
    current: DateRange;
    compare?: DateRange;
}

export default function CommentAnalytics(props: CommentAnalyticsProps) {
    const { seriesId, current, compare } = props;
    const [commentRanking, setCommentRanking] = useState<RankingResp>();
    const [commentAnalytics, setCommentAnalytics] =
        useState<CommentAnalyticsResp>();
    const [commentSearch, setCommentSearch] = useState<string>("");
    const [commentPage, setCommentPage] = useState<number>(1);
    const [commentDesc, setCommentDesc] = useState<boolean>(false);

    const currentStr = formatDateRange(current);
    const compareStr = compare ? formatDateRange(compare) : "N/A";

    useEffect(() => {
        async function fetchComment() {
            const [data, err] = await getCommentAnalytics({
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
            setCommentAnalytics(data);
        }
        Promise.all([fetchComment()]);
    }, [current, compare, seriesId]);

    useEffect(() => {
        async function fetchCommentRanking() {
            const [data, err] = await getCommentRanking({
                current,
                compare,
                seriesId,
                desc: commentDesc,
                pageNumber: commentPage,
                query: commentSearch,
            });
            if (err) {
                toast.error(
                    "Có lỗi xảy ra khi tải dữ liệu lượt xem. Vui lòng thử lại sau",
                );
                return;
            }
            setCommentRanking(data);
        }

        fetchCommentRanking();
    }, [commentPage, commentSearch, commentDesc, seriesId, current, compare]);

    return (
        <div className="mt-2 flex flex-wrap gap-2">
            <OmniChart
                data={commentAnalytics!}
                ranking={commentRanking?.items}
                formatter={(e) => e.toString()}
                titleGeneral={
                    <>
                        <MessageSquareText />
                        Bình luận - Tổng quan
                    </>
                }
                descGeneral="Thống kê tổng quan bình luận tác phẩm trong thời gian qua"
                titleRanking={
                    <>
                        <MessageSquareText />
                        Bình luận - So sánh
                    </>
                }
                descRanking="Thống kê so sánh bình luận của các chương truyện trong tác phẩm trong thời gian qua"
                current={currentStr}
                compare={compareStr}
                onPageChange={(page) => setCommentPage(page)}
                onSearch={(query) => setCommentSearch(query)}
                onSort={() => setCommentDesc(!commentDesc)}
                rankingCurrentPage={commentRanking?.pageNumber ?? 1}
                rankingTotalPages={commentRanking?.totalPages ?? 10}
                search={commentSearch}
            />
        </div>
    );
}
