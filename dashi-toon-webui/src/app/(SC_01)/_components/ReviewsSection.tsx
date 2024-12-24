import { ReportDialog } from "@/components/ReportDialog";
import { Avatar, AvatarFallback, AvatarImage } from "@/components/ui/avatar";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardFooter, CardHeader } from "@/components/ui/card";
import {
    Pagination,
    PaginationContent,
    PaginationItem,
    PaginationLink,
    PaginationNext,
    PaginationPrevious,
} from "@/components/ui/pagination";
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/ui/select";
import { Textarea } from "@/components/ui/textarea";
import { formatUpdatedAt } from "@/lib/date-fns";
import { reportReview } from "@/utils/api/reader/report";
import {
    createSeriesReview,
    getCurrentUserSeriesReview,
    getSeriesReviews,
    rateSeriesReview,
    ReviewSortBy,
    SeriesReview,
    updateSeriesReview,
} from "@/utils/api/reader/review";
import { Analytics, getSeriesInfo, SeriesResp } from "@/utils/api/series";
import { getUserInfo, UserSession } from "@/utils/api/user";
import { ChevronDown, ThumbsDown, ThumbsUp } from "lucide-react";
import Link from "next/link";
import React, { useEffect, useState } from "react";
import { toast } from "sonner";
import { isAllowedToReviewOrComment } from "@/utils/api/moderator/moderation";
import { AlertCircle } from "lucide-react";

export interface ReviewSectionProps {
    seriesId: string;
    seriesTitle: string | undefined;
    analytics?: Analytics;
    onReviewUpdate?: () => Promise<void>;
}

interface ReviewPermission {
    isAllowed: boolean;
    notAllowedUntil?: string;
}

export default function ReviewsSection({ seriesId, seriesTitle, analytics, onReviewUpdate }: ReviewSectionProps) {
    const [seriesInfo, setSeriesInfo] = useState<SeriesResp | null>(null);
    const [session, setSession] = useState<UserSession | null>(null);
    const [expanded, setExpanded] = useState<string[]>([]);
    const [reviews, setReviews] = useState<SeriesReview[]>([]);
    const [newReview, setNewReview] = useState("");
    const [isRecommended, setIsRecommended] = useState<boolean | null>(null);
    const [currentUserReview, setCurrentUserReview] = useState<SeriesReview | null>(null);
    const [isEditing, setIsEditing] = useState(false);
    const [currentPage, setCurrentPage] = useState(1);
    const [totalPages, setTotalPages] = useState(1);
    const [sortBy, setSortBy] = useState<ReviewSortBy>("Relevance");
    const pageSize = 5;
    const [reviewPermission, setReviewPermission] = useState<ReviewPermission | null>(null);

    useEffect(() => {
        const fetchSeriesInfo = async () => {
            const [data, error] = await getSeriesInfo(seriesId);
            if (error) {
                toast.error("Không thể tải thông tin về series");
                return;
            }
            if (data) {
                setSeriesInfo(data);
            }
        };
        fetchSeriesInfo();
    }, [seriesId]);

    useEffect(() => {
        async function work() {
            const [s, e] = await getUserInfo();
            if (e) {
                setSession(null);
                return;
            }
            setSession(s);
        }
        work();
    }, []);

    useEffect(() => {
        const fetchReviews = async () => {
            const [data, error] = await getSeriesReviews(seriesId, currentPage, pageSize, sortBy);
            if (error) {
                toast.error("Không thể tải các bài đánh giá");
                return;
            }
            if (data) {
                setReviews(data.items);
                setTotalPages(data.totalPages);
            }
        };
        fetchReviews();
    }, [seriesId, currentPage, sortBy]);

    useEffect(() => {
        const fetchUserReview = async () => {
            if (!session) return;
            const [data, error] = await getCurrentUserSeriesReview(seriesId);
            if (error) {
                toast.error("Không thể tải bài đánh giá của bạn");
                return;
            }
            if (data) {
                setCurrentUserReview(data);
                setNewReview(data.content);
                setIsRecommended(data.isRecommended);
            }
        };
        fetchUserReview();
    }, [seriesId, session]);

    useEffect(() => {
        const checkReviewPermission = async () => {
            const [data, error] = await isAllowedToReviewOrComment();
            if (!error && data) {
                setReviewPermission(data);
            }
        };
        checkReviewPermission();
    }, []);

    const handleTextareaChange = (e: React.ChangeEvent<HTMLTextAreaElement>) => {
        setNewReview(e.target.value);
    };

    const handleKeyDown = (e: React.KeyboardEvent<HTMLTextAreaElement>) => {
        if (e.key === "Enter" && !e.shiftKey) {
            e.preventDefault();
            handleSubmitReview();
        }
    };

    const handleSubmitReview = async () => {
        if (isRecommended === null || !newReview.trim()) return;
        if (!reviewPermission?.isAllowed) {
            toast.error("Bạn không có quyền đánh giá");
            return;
        }

        const [data, error] = await createSeriesReview(seriesId, newReview, isRecommended);
        if (error) {
            toast.error("Không thể gửi bài đánh giá");
            return;
        }
        if (data) {
            setCurrentUserReview(data);
            const [seriesData] = await getSeriesInfo(seriesId);
            if (seriesData) {
                setSeriesInfo(seriesData);
            }

            const [reviewsData] = await getSeriesReviews(seriesId, currentPage, pageSize, sortBy);
            if (reviewsData) {
                setReviews(reviewsData.items);
                setTotalPages(reviewsData.totalPages);
            }

            if (onReviewUpdate) {
                await onReviewUpdate();
            }

            toast.success("Đã gửi bài đánh giá thành công!");
        }
    };

    const toggleExpand = (id: string) => {
        setExpanded((prev) => (prev.includes(id) ? prev.filter((i) => i !== id) : [...prev, id]));
    };

    const handleUpdate = async () => {
        if (isRecommended === null || !newReview.trim() || !currentUserReview) return;
        if (!reviewPermission?.isAllowed) {
            toast.error("Bạn không có quyền đánh giá");
            return;
        }

        const [data, error] = await updateSeriesReview(seriesId, currentUserReview.id, newReview, isRecommended);

        if (error) {
            toast.error("Không thể cập nhật bài đánh giá");
            return;
        }
        if (data) {
            setCurrentUserReview(data);
            setIsEditing(false);
            const [seriesData] = await getSeriesInfo(seriesId);
            if (seriesData) {
                setSeriesInfo(seriesData);
            }

            const [reviewsData] = await getSeriesReviews(seriesId, currentPage, pageSize, sortBy);
            if (reviewsData) {
                setReviews(reviewsData.items);
                setTotalPages(reviewsData.totalPages);
            }

            if (onReviewUpdate) {
                await onReviewUpdate();
            }
            toast.success("Đã cập nhật bài đánh giá thành công!");
        }
    };

    const handleCancel = () => {
        setIsEditing(false);
        if (currentUserReview) {
            setNewReview(currentUserReview.content || "");
            setIsRecommended(currentUserReview.isRecommended || null);
        }
    };

    const handleSortChange = (value: string) => {
        setSortBy(value as ReviewSortBy);
        setCurrentPage(1);
    };

    const handleRateReview = async (reviewId: string, isLiked: boolean) => {
        const [data, error] = await rateSeriesReview(seriesId, reviewId, isLiked);
        if (error) {
            toast.error("Lỗi khi đánh giá bài đánh giá!");
            return;
        }
        if (data) {
            setReviews(reviews.map((r) => (r.id === reviewId ? data : r)));
        }
    };

    const handleReport = async (
        contentId: string,
        reason: string,
        contentType: "series" | "chapter" | "review" | "comment",
    ) => {
        try {
            const [error] = await reportReview(contentId, reason);
            if (error) {
                toast.error("Không thể gửi báo cáo. Vui lòng thử lại sau");
                return;
            }
            toast.success("Đã gửi báo cáo thành công");
        } catch (error) {
            console.error("Error:", error);
            toast.error("Đã xảy ra lỗi khi gửi báo cáo");
        }
    };

    return (
        <div className="container mx-auto p-16 text-gray-200">
            <Card className="border-neutral-700 bg-neutral-800">
                <CardHeader>
                    <h2 className="text-center text-sm font-semibold text-gray-400">
                        {currentUserReview && !isEditing ? "Bài đánh giá của bạn" : "Viết đánh giá"}
                    </h2>
                    <p className="mt-2 text-center font-bold">Bạn có đề xuất {seriesTitle} không?</p>
                    {session && (!currentUserReview || isEditing) && (
                        <div className="mt-4 flex justify-center space-x-4">
                            <div
                                className={`flex h-16 w-16 cursor-pointer items-center justify-center rounded-full ${
                                    isRecommended === true ? "bg-green-500" : "bg-neutral-700 hover:bg-green-500/20"
                                }`}
                                onClick={() => setIsRecommended(true)}
                            >
                                <ThumbsUp
                                    className={`h-9 w-9 ${isRecommended === true ? "text-white" : "text-gray-400"}`}
                                />
                            </div>
                            <div
                                className={`flex h-16 w-16 cursor-pointer items-center justify-center rounded-full ${
                                    isRecommended === false ? "bg-red-700" : "bg-neutral-700 hover:bg-red-700/20"
                                }`}
                                onClick={() => setIsRecommended(false)}
                            >
                                <ThumbsDown
                                    className={`h-9 w-9 ${isRecommended === false ? "text-white" : "text-gray-400"}`}
                                />
                            </div>
                        </div>
                    )}
                </CardHeader>
                <CardContent>
                    {!session ? (
                        <div className="text-center text-neutral-400">
                            <p>Vui lòng đăng nhập để viết đánh giá</p>
                        </div>
                    ) : !reviewPermission?.isAllowed ? (
                        <div className="rounded-lg border border-red-800 bg-red-950/50 p-4">
                            <div className="flex items-center gap-3 text-red-400">
                                <AlertCircle className="h-5 w-5" />
                                <div className="space-y-1">
                                    <h3 className="font-medium">Tài khoản của bạn đã bị hạn chế đánh giá</h3>
                                    {reviewPermission?.notAllowedUntil && (
                                        <p className="text-sm">
                                            Bạn có thể đánh giá lại sau:{" "}
                                            {new Date(reviewPermission.notAllowedUntil).toLocaleDateString("vi-VN", {
                                                year: "numeric",
                                                month: "long",
                                                day: "numeric",
                                                hour: "2-digit",
                                                minute: "2-digit",
                                            })}
                                        </p>
                                    )}
                                </div>
                            </div>
                        </div>
                    ) : currentUserReview && !isEditing ? (
                        <div>
                            <Textarea
                                disabled
                                className="min-h-[125px] border-neutral-600 bg-neutral-700"
                                value={currentUserReview.content || ""}
                            />
                            <div className="mt-2 flex items-center">
                                {currentUserReview.isRecommended ? (
                                    <>
                                        <ThumbsUp className="mr-2 h-6 w-6 text-green-500" />
                                        <span className="text-sm text-green-500">Đề xuất</span>
                                    </>
                                ) : (
                                    <>
                                        <ThumbsDown className="mr-2 h-6 w-6 text-red-500" />
                                        <span className="text-sm text-red-500">Không đề xuất</span>
                                    </>
                                )}
                            </div>
                        </div>
                    ) : (
                        <div className="relative">
                            <Textarea
                                placeholder="Viết đánh giá ít nhất 100 chữ... (Shift + Enter để xuống dòng)"
                                className="min-h-[125px] border-neutral-600 bg-neutral-700 pb-8"
                                value={newReview}
                                onChange={handleTextareaChange}
                                onKeyDown={handleKeyDown}
                            />
                            <div className="absolute bottom-2 right-2 text-sm text-neutral-400">
                                <span
                                    className={
                                        newReview.trim().length < 100 || newReview.trim().length > 8192
                                            ? "text-red-400"
                                            : ""
                                    }
                                >
                                    {newReview.trim().length}
                                </span>
                                /{8192}
                            </div>
                        </div>
                    )}
                </CardContent>
                <CardFooter>
                    {!session ? (
                        <Link
                            href="/login"
                            className="w-full text-center text-blue-500 hover:text-blue-400 hover:underline"
                        >
                            Đăng nhập để đánh giá
                        </Link>
                    ) : currentUserReview ? (
                        isEditing ? (
                            <div className="flex w-full justify-end gap-2">
                                <Button
                                    className="w-fit bg-blue-600 text-white hover:bg-blue-700"
                                    onClick={handleUpdate}
                                    disabled={
                                        isRecommended === null ||
                                        !newReview.trim() ||
                                        newReview.trim().length < 100 ||
                                        newReview.trim().length > 8192
                                    }
                                >
                                    Cập nhật
                                </Button>
                                <Button className="w-24" variant="outline" onClick={handleCancel}>
                                    Hủy
                                </Button>
                            </div>
                        ) : (
                            <div className="flex w-full justify-end">
                                <Button
                                    className="w-fit bg-blue-600 text-white hover:bg-blue-700"
                                    onClick={() => setIsEditing(true)}
                                >
                                    Chỉnh sửa đánh giá
                                </Button>
                            </div>
                        )
                    ) : (
                        <div className="flex w-full justify-end">
                            <Button
                                className="w-fit bg-blue-600 hover:bg-blue-700"
                                onClick={handleSubmitReview}
                                disabled={
                                    isRecommended === null ||
                                    !newReview.trim() ||
                                    newReview.trim().length < 100 ||
                                    newReview.trim().length > 8192
                                }
                            >
                                Gửi
                            </Button>
                        </div>
                    )}
                </CardFooter>
            </Card>

            <div className="mt-6 flex items-center justify-between">
                <div className="flex items-center">
                    <ThumbsUp className="mr-2 h-8 w-8" />
                    <span className="font-bold">{analytics?.rating}%</span>
                    <span className="ml-2 text-neutral-400">{analytics?.reviewCount} Đánh giá</span>
                </div>
                <Select defaultValue="Relevance" onValueChange={handleSortChange}>
                    <SelectTrigger className="w-[180px] border-neutral-700 bg-neutral-800">
                        <SelectValue placeholder="Sắp xếp theo" />
                    </SelectTrigger>
                    <SelectContent className="border-neutral-700 bg-neutral-800">
                        <SelectItem value="Relevance">Phù hợp nhất</SelectItem>
                        <SelectItem value="Best">Đánh giá tốt nhất</SelectItem>
                        <SelectItem value="Worst">Đánh giá tệ nhất</SelectItem>
                        <SelectItem value="Newest">Mới nhất</SelectItem>
                        <SelectItem value="Oldest">Cũ nhất</SelectItem>
                    </SelectContent>
                </Select>
            </div>

            <div className="mt-4 space-y-4">
                {reviews.length > 0 &&
                    reviews.map((review) => (
                        <Card key={review.id} className="border-neutral-700 bg-neutral-800">
                            <CardHeader>
                                <div className="flex items-center">
                                    <Avatar className="mr-4 h-14 w-14 rounded-none">
                                        <AvatarImage src={review.userAvatar} />
                                        <AvatarFallback>{review.username[0]}</AvatarFallback>
                                    </Avatar>
                                    <div>
                                        <h3 className="font-semibold">{review.username}</h3>
                                        <p className="text-sm text-neutral-400">{formatUpdatedAt(review.reviewDate)}</p>
                                        {review.isRecommended ? (
                                            <div className="mt-2 flex items-center font-bold italic text-green-500">
                                                <ThumbsUp className="mr-2 h-4 w-4" />
                                                <span className="text-sm">Đề xuất bộ truyện này!</span>
                                            </div>
                                        ) : (
                                            <div className="mt-2 flex items-center font-bold italic text-red-500">
                                                <ThumbsDown className="mr-2 h-4 w-4" />
                                                <span className="text-sm">Không đề xuất bộ truyện này!</span>
                                            </div>
                                        )}
                                    </div>
                                </div>
                            </CardHeader>
                            <CardContent>
                                <p
                                    id={`review-${review.id}`}
                                    className={`break-words ${expanded.includes(review.id) ? "" : "line-clamp-2"}`}
                                >
                                    {review.content.split("\n").map((line, index) => (
                                        <React.Fragment key={index}>
                                            {line}
                                            {index < review.content.split("\n").length - 1 && <br />}
                                        </React.Fragment>
                                    ))}
                                </p>
                                <span
                                    className="flex cursor-pointer items-center text-gray-400 hover:underline"
                                    onClick={() => toggleExpand(review.id)}
                                >
                                    Xem thêm
                                    <ChevronDown
                                        className={`ml-2 h-4 w-4 transition-transform ${
                                            expanded.includes(review.id) ? "rotate-180" : ""
                                        }`}
                                    />
                                </span>
                            </CardContent>
                            <CardFooter className="flex items-center justify-between">
                                <div className="flex items-center space-x-4">
                                    <div
                                        className="group flex cursor-pointer items-center gap-1"
                                        onClick={() => handleRateReview(review.id, true)}
                                    >
                                        <ThumbsUp className={`h-4 w-4 group-hover:text-blue-500`} />
                                        <span className={`text-sm group-hover:text-blue-500`}>{review.likes}</span>
                                    </div>
                                    <div
                                        className="group flex cursor-pointer items-center gap-1"
                                        onClick={() => handleRateReview(review.id, false)}
                                    >
                                        <ThumbsDown className={`h-4 w-4 group-hover:text-blue-500`} />
                                        <span className={`text-sm group-hover:text-blue-500`}>{review.dislikes}</span>
                                    </div>
                                    <ReportDialog
                                        contentId={review.id}
                                        contentType="review"
                                        username={review.username}
                                        onReport={handleReport}
                                    />
                                </div>
                            </CardFooter>
                        </Card>
                    ))}
                <div className="mt-6 flex justify-center">
                    {reviews.length > 0 && (
                        <Pagination>
                            <PaginationContent>
                                {currentPage > 1 ? (
                                    <PaginationItem>
                                        <PaginationPrevious onClick={() => setCurrentPage(currentPage - 1)} />
                                    </PaginationItem>
                                ) : (
                                    <PaginationItem>
                                        <PaginationPrevious className="pointer-events-none opacity-50" />
                                    </PaginationItem>
                                )}

                                {Array.from({ length: totalPages }, (_, i) => i + 1).map((page) => (
                                    <PaginationItem key={page}>
                                        <PaginationLink
                                            onClick={() => setCurrentPage(page)}
                                            isActive={page === currentPage}
                                        >
                                            {page}
                                        </PaginationLink>
                                    </PaginationItem>
                                ))}
                                {currentPage < totalPages ? (
                                    <PaginationItem>
                                        <PaginationNext onClick={() => setCurrentPage(currentPage + 1)} />
                                    </PaginationItem>
                                ) : (
                                    <PaginationItem>
                                        <PaginationNext className="pointer-events-none opacity-50" />
                                    </PaginationItem>
                                )}
                            </PaginationContent>
                        </Pagination>
                    )}
                </div>
            </div>
        </div>
    );
}
