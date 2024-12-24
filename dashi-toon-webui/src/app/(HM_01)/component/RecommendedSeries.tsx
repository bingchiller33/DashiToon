import SeriesCard from "@/components/SeriesCard";
import { Button } from "@/components/ui/button";
import { Skeleton } from "@/components/ui/skeleton";
import useUwU from "@/hooks/useUwU";
import { getRecommendedSeries } from "@/utils/api/series";
import { useInfiniteQuery } from "@tanstack/react-query";
import { Loader2, Sparkles } from "lucide-react";
import React, { useEffect } from "react";
import { useInView } from "react-intersection-observer";

export default function RecommendedSeries() {
    const [uwu] = useUwU();
    const { ref, inView } = useInView();
    const { data, fetchNextPage, hasNextPage, isFetchingNextPage, status } = useInfiniteQuery({
        initialPageParam: 1,
        queryKey: ["recommendedSeries"],
        queryFn: async ({ pageParam = 1 }) => {
            console.log({ pageParam });
            const [data, err] = await getRecommendedSeries(pageParam, 24);
            if (err) {
                throw new Error("Failed to fetch recommended series");
            }

            return data;
        },
        getNextPageParam: (lastPage) => {
            const next = lastPage.pageNumber + 1;
            return next <= lastPage.totalPages ? next : undefined;
        },
    });

    useEffect(() => {
        if (inView && hasNextPage) {
            fetchNextPage();
        }
    }, [inView, fetchNextPage, hasNextPage]);

    const firstLoad = isFetchingNextPage && !data;

    return (
        <div>
            <div className="bg-gradient-to-t from-transparent to-blue-500/30">
                <div className="container flex flex-col items-center py-12 pt-20">
                    <div className="sparkles mb-4 h-16 w-16"></div>
                    <h2 className="text-bold mb-2 text-center text-3xl">
                        {uwu ? "Doom Scroll?" : "Tiếp tục cuộn để khám phá thêm các đề xuất"}
                    </h2>
                    <p className="text-center text-muted-foreground">
                        Bên dưới, bạn sẽ tìm thấy nhiều bộ truyện mà bạn có thể quan tâm từ kho tàng truyện trên
                        DashiToon!
                    </p>
                </div>
            </div>
            <div className="container grid gap-6 md:grid-cols-2 lg:grid-cols-4 xl:grid-cols-6">
                {firstLoad ? (
                    Array.from({ length: 24 }).map((_, i) => <Skeleton key={i} className="aspect-[3/4]" />)
                ) : status === "error" ? (
                    <p>Failed to fetch recommended series</p>
                ) : (
                    data?.pages.map((page, i) => (
                        <React.Fragment key={i}>
                            {page?.items?.map((series) => <SeriesCard key={series.id} data={series} />)}
                        </React.Fragment>
                    ))
                )}
            </div>
            <div ref={ref} className="mt-4 flex h-20 items-center justify-center">
                {isFetchingNextPage ? (
                    <Loader2 className="h-6 w-6 animate-spin" />
                ) : hasNextPage ? (
                    <Button onClick={() => fetchNextPage()} className="bg-blue-600 hover:bg-blue-700">
                        Tải thêm
                    </Button>
                ) : (
                    <p>Không còn kết quả nào khác.</p>
                )}
            </div>
        </div>
    );
}
