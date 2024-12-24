import React from "react";
import {
    Pagination,
    PaginationContent,
    PaginationEllipsis,
    PaginationItem,
    PaginationLink,
    PaginationNext,
    PaginationPrevious,
} from "@/components/ui/pagination";

import { getPagination } from "@/utils";

export interface ChapterPaginationProps {
    totalPages: number;
    currentPage: number;
    pageLink: (page: number) => string;
}

export default function ChapterPagination(props: ChapterPaginationProps) {
    const { totalPages, currentPage, pageLink } = props;
    const items = getPagination(currentPage, totalPages);

    return (
        <Pagination className="justify-end">
            <PaginationContent>
                <PaginationItem>
                    <PaginationPrevious
                        href={currentPage > 1 ? pageLink(currentPage - 1) : "#"}
                        aria-disabled={currentPage === 1}
                    />
                </PaginationItem>
                {items.map((page, index) => (
                    <PaginationItem key={index}>
                        {page === "..." ? (
                            <PaginationEllipsis />
                        ) : (
                            <PaginationLink
                                href={pageLink(page as number)}
                                isActive={page === currentPage}
                            >
                                {page}
                            </PaginationLink>
                        )}
                    </PaginationItem>
                ))}
                <PaginationItem>
                    <PaginationNext
                        href={
                            currentPage === totalPages
                                ? pageLink(currentPage + 1)
                                : "#"
                        }
                        aria-disabled={currentPage === totalPages}
                    />
                </PaginationItem>
            </PaginationContent>
        </Pagination>
    );
}
