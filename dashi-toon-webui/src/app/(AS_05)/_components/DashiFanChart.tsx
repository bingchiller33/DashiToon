"use client";

import { ArrowUpDown, ChartNoAxesColumn, Hash, TrendingUp } from "lucide-react";

import { Button } from "@/components/ui/button";
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card";
import { Input } from "@/components/ui/input";
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from "@/components/ui/table";
import { Analytics, ChapterRanking, DashiFanAnalyticsResp, TierRanking } from "@/utils/api/analytics";
import Link from "next/link";
import { ReactNode } from "react";
import Insight from "./Insight";
import OmniAreaSubChart from "./OmniAreaSubChart";
import OmniPieSubChart from "./OmniPieSubChart";
import OmniRankingPagination from "./OmniRankingPagination";
import StatNum from "./StatNum";
import { Skeleton } from "@/components/ui/skeleton";

export interface DashiFanChartProps {
    data?: DashiFanAnalyticsResp;
    ranking?: TierRanking[];
    rankingCurrentPage: number;
    rankingTotalPages: number;
    onPageChange: (page: number) => void;
    onSort: () => void;
    search: string;
    onSearch: (term: string) => void;
    current: string;
    compare: string;
    titleGeneral: ReactNode;
    descGeneral: ReactNode;
    titleRanking: ReactNode;
    descRanking: ReactNode;
    formatter: (value: number) => string;
}

export function DashiFanChart(props: DashiFanChartProps) {
    const {
        data,
        ranking,
        rankingCurrentPage,
        rankingTotalPages,
        onPageChange,
        onSort,
        search,
        onSearch,
        formatter,
        current,
        compare,
        titleGeneral,
        descGeneral,
        titleRanking,
        descRanking,
    } = props;
    return (
        <>
            <Card className="flex-shrink flex-grow-[2] basis-[500px]">
                <CardHeader>
                    <CardTitle className="flex gap-2">{titleGeneral}</CardTitle>
                    <CardDescription>{descGeneral}</CardDescription>
                </CardHeader>
                <CardContent>
                    {data ? (
                        <div>
                            <OmniAreaSubChart data={data.data} current={current} compare={compare} />
                        </div>
                    ) : (
                        <Skeleton className="h-[300px] w-full" />
                    )}
                    <div className="mt-4 grid gap-8 sm:grid-cols-3">
                        <Insight
                            icon={<Hash className="h-6 w-6 text-blue-400" />}
                            isLoading={!data}
                            title="Tổng cộng toàn thời gian"
                            current={data?.total ?? 0}
                            compare={data?.totalCompare ?? 0}
                            formatter={formatter}
                        />
                        <Insight
                            icon={<ChartNoAxesColumn className="h-6 w-6 text-green-400" />}
                            isLoading={!data}
                            title="Thứ hạng trên hệ thống"
                            current={data?.ranking ?? 0}
                            compare={data?.rankingCompare ?? 0}
                            prefix="#"
                        />
                        <Insight
                            icon={<TrendingUp className="h-6 w-6 text-orange-400" />}
                            isLoading={!data}
                            title="Tăng trưởng"
                            current={data?.growth ?? 0}
                            compare={data?.growthCompare ?? 0}
                            formatter={(v) => v.toFixed(1) + "%"}
                        />
                    </div>
                </CardContent>
            </Card>

            <Card className="max-w-[100vw] flex-grow">
                <CardHeader>
                    <CardTitle className="flex gap-2">{titleRanking}</CardTitle>
                    <CardDescription>{descRanking}</CardDescription>
                </CardHeader>
                <CardContent className="gap-2 md:flex">
                    <div className="flex items-center">
                        {data ? (
                            <OmniPieSubChart current={current} compare={compare} data={data.topTiers as any} />
                        ) : (
                            <Skeleton className="h-[388px] w-[300px]" />
                        )}
                    </div>

                    <div className="flex-grow">
                        <div className="flex items-center justify-end gap-2">
                            <Input
                                value={search}
                                onChange={(e) => onSearch(e.target.value)}
                                placeholder="🔍 Lọc theo tên"
                                className="mb-2 w-[200px] flex-shrink border-neutral-600 bg-neutral-700 text-neutral-100"
                            />
                        </div>
                        {!ranking ? (
                            <Skeleton className="h-[300px] w-full" />
                        ) : (
                            <div className="relative ms-auto block h-[300px] w-full overflow-auto">
                                <Table>
                                    <TableHeader
                                        className="sticky left-0 top-0"
                                        style={{
                                            background: "hsl(0deg 0% 14.9%)",
                                        }}
                                    >
                                        <TableRow>
                                            <TableHead className="text-neutral-300">
                                                <Button
                                                    variant="ghost"
                                                    onClick={() => onSort()}
                                                    className="h-auto p-0 font-bold text-neutral-300 hover:text-blue-300"
                                                >
                                                    #
                                                    <ArrowUpDown className="ml-2 h-4 w-4" />
                                                </Button>
                                            </TableHead>
                                            <TableHead className="min-w-[180px] text-neutral-300">Tên hạng</TableHead>
                                            <TableHead className="min-w-[96px] text-neutral-300">Lượt</TableHead>
                                        </TableRow>
                                    </TableHeader>
                                    <TableBody>
                                        {ranking.map((sub) => (
                                            <TableRow key={sub.id}>
                                                <TableCell className="text-neutral-100">{sub.rank}</TableCell>
                                                <TableCell className="text-neutral-100">
                                                    <Link
                                                        href={`/author-studio/series/1/dashifan/${sub.id}`}
                                                        className="underline hover:text-blue-300"
                                                    >
                                                        {sub.name}
                                                    </Link>
                                                </TableCell>
                                                <TableCell className="text-neutral-100">
                                                    <StatNum current={sub.data} compare={sub.compare} isGood={true} />
                                                </TableCell>
                                            </TableRow>
                                        ))}
                                        {ranking.length === 0 && (
                                            <div className="absolute left-1/2 top-1/2 -translate-x-1/2 -translate-y-1/2 transform text-neutral-300">
                                                Không có dữ liệu
                                            </div>
                                        )}
                                    </TableBody>
                                </Table>
                            </div>
                        )}

                        {!ranking ? (
                            <Skeleton className="h-[40px] w-full" />
                        ) : (
                            <OmniRankingPagination
                                totalPages={rankingCurrentPage}
                                currentPage={rankingTotalPages}
                                onClick={(e) => onPageChange(e)}
                            />
                        )}
                    </div>
                </CardContent>
            </Card>
        </>
    );
}
