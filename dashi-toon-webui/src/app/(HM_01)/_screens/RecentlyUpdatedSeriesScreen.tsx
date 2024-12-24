"use client";

import { useState, useEffect } from "react";
import NovelChaptersTable from "@/components/UpdatedSeriesTable";
import SiteLayout from "@/components/SiteLayout";
import {
    Breadcrumb,
    BreadcrumbItem,
    BreadcrumbLink,
    BreadcrumbList,
    BreadcrumbPage,
    BreadcrumbSeparator,
} from "@/components/ui/breadcrumb";
import { Home } from "lucide-react";
import { toast } from "sonner";
import { useFetchApi } from "@/hooks/useFetchApi";
import { getRecentlyUpdatedSeries } from "@/utils/api/series";

export default function RecentlyUpdatedSeriesScreen() {
    const [updated, updatedLoading] = useFetchApi(getRecentlyUpdatedSeries);

    return (
        <SiteLayout>
            <div className="container py-6">
                <Breadcrumb className="mb-2">
                    <BreadcrumbList>
                        <BreadcrumbItem>
                            <BreadcrumbLink href="/" className="flex items-center">
                                <Home className="mr-2 h-4 w-4" />
                            </BreadcrumbLink>
                        </BreadcrumbItem>
                        <BreadcrumbSeparator />
                        <BreadcrumbItem>
                            <BreadcrumbPage>Truyện vừa cập nhật</BreadcrumbPage>
                        </BreadcrumbItem>
                    </BreadcrumbList>
                </Breadcrumb>
                <h1 className="mb-6 text-3xl font-bold">Truyện vừa cập nhật</h1>
                <NovelChaptersTable chapters={updated ?? []} isLoading={updatedLoading} />
            </div>
        </SiteLayout>
    );
}
