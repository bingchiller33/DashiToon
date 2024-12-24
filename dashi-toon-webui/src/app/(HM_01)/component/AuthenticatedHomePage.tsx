import React from "react";
import CarouselComponent from "./CarouselComponent";
import { ChevronRight } from "lucide-react";
import { useFetchApi } from "@/hooks/useFetchApi";
import { getRecentlyReleasedSeries, getRecommendedSeries } from "@/utils/api/series";
import Link from "next/link";
import { getFollowedSeries } from "@/utils/api/user";
import ContinueCarousel from "./ContinueCarousel";
import { Button } from "@/components/ui/button";

export default function AuthenticatedHomePage({ onClick }: { onClick: () => void }) {
    const [follows, followsLoading] = useFetchApi(getFollowedSeries.bind(null, 1, 12));
    const [recommended, recommendedLoading] = useFetchApi(getRecommendedSeries);

    return (
        <>
            {(follows?.items.length ?? 0) > 0 && (
                <>
                    <ContinueCarousel data={follows?.items ?? []} heading="Tiếp Tục Đọc" isLoading={followsLoading} />
                    <div className="flex justify-end pb-10">
                        <Button
                            variant={"link"}
                            className="flex items-center gap-2 italic text-muted-foreground underline hover:text-blue-400"
                            asChild
                        >
                            <Link href="/following">
                                Xem tất cả <ChevronRight />
                            </Link>
                        </Button>
                    </div>
                </>
            )}

            <CarouselComponent data={recommended?.items ?? []} heading="Dành Cho Bạn" isLoading={recommendedLoading} />
            <div className="flex justify-end pb-10">
                <Button
                    onClick={onClick}
                    variant={"link"}
                    className="flex items-center gap-2 italic text-muted-foreground underline hover:text-blue-400"
                >
                    Xem thêm <ChevronRight />
                </Button>
            </div>
        </>
    );
}
