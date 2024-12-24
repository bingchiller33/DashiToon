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

export interface RevenuePaginationProps {
    totalPages: number;
    currentPage: number;
    onClick: (page: number) => void;
}

export default function RevenuePagination(props: RevenuePaginationProps) {
    const { totalPages, currentPage, onClick } = props;
    const items = getPagination(currentPage, totalPages);

    return (
        <Pagination className="justify-end">
            <PaginationContent>
                <PaginationItem>
                    <PaginationPrevious
                        onClick={() =>
                            currentPage > 1 && onClick(currentPage - 1)
                        }
                        aria-disabled={currentPage === 1}
                    />
                </PaginationItem>
                {items.map((page, index) => (
                    <PaginationItem key={index}>
                        {page === "..." ? (
                            <PaginationEllipsis />
                        ) : (
                            <PaginationLink
                                onClick={() => onClick(page as number)}
                                isActive={page === currentPage}
                            >
                                {page}
                            </PaginationLink>
                        )}
                    </PaginationItem>
                ))}
                <PaginationItem>
                    <PaginationNext
                        onClick={() =>
                            currentPage === totalPages &&
                            onClick(currentPage + 1)
                        }
                        aria-disabled={currentPage === totalPages}
                    />
                </PaginationItem>
            </PaginationContent>
        </Pagination>
    );
}
