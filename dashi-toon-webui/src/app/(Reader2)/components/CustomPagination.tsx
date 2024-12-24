import {
    Pagination,
    PaginationContent,
    PaginationEllipsis,
    PaginationItem,
    PaginationLink,
    PaginationNext,
    PaginationPrevious,
} from "@/components/ui/pagination";

interface CustomPaginationProps {
    currentPage: number;
    totalPages: number;
    hasNextPage: boolean;
    onPageChange: (page: number) => void;
}

export function CustomPagination({
    currentPage,
    totalPages,
    hasNextPage,
    onPageChange,
}: CustomPaginationProps) {
    return (
        <Pagination>
            <PaginationContent>
                <PaginationItem>
                    <PaginationPrevious
                        onClick={() => {
                            if (currentPage > 1) {
                                onPageChange(currentPage - 1);
                            }
                        }}
                        className={
                            currentPage <= 1
                                ? "pointer-events-none opacity-50"
                                : ""
                        }
                    />
                </PaginationItem>

                {/* First Page */}
                {currentPage > 2 && (
                    <PaginationItem>
                        <PaginationLink onClick={() => onPageChange(1)}>
                            1
                        </PaginationLink>
                    </PaginationItem>
                )}

                {/* Ellipsis */}
                {currentPage > 3 && (
                    <PaginationItem>
                        <PaginationEllipsis />
                    </PaginationItem>
                )}

                {/* Previous Page */}
                {currentPage > 1 && (
                    <PaginationItem>
                        <PaginationLink
                            onClick={() => onPageChange(currentPage - 1)}
                        >
                            {currentPage - 1}
                        </PaginationLink>
                    </PaginationItem>
                )}

                {/* Current Page */}
                <PaginationItem>
                    <PaginationLink isActive>{currentPage}</PaginationLink>
                </PaginationItem>

                {/* Next Page */}
                {hasNextPage && (
                    <PaginationItem>
                        <PaginationLink
                            onClick={() => onPageChange(currentPage + 1)}
                        >
                            {currentPage + 1}
                        </PaginationLink>
                    </PaginationItem>
                )}

                {/* Ellipsis */}
                {currentPage < totalPages - 2 && (
                    <PaginationItem>
                        <PaginationEllipsis />
                    </PaginationItem>
                )}

                {/* Last Page */}
                {currentPage < totalPages - 1 && (
                    <PaginationItem>
                        <PaginationLink
                            onClick={() => onPageChange(totalPages)}
                        >
                            {totalPages}
                        </PaginationLink>
                    </PaginationItem>
                )}

                <PaginationItem>
                    <PaginationNext
                        onClick={() => {
                            if (hasNextPage) {
                                onPageChange(currentPage + 1);
                            }
                        }}
                        className={
                            !hasNextPage ? "pointer-events-none opacity-50" : ""
                        }
                    />
                </PaginationItem>
            </PaginationContent>
        </Pagination>
    );
}
