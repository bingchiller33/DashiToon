import React from "react";
import { Carousel, CarouselContent, CarouselItem, CarouselNext, CarouselPrevious } from "@/components/ui/carousel";
import { Skeleton } from "@/components/ui/skeleton";
import SeriesCard from "@/components/SeriesCard";
import { Bird, BookOpenText } from "lucide-react";

export default function CarouselComponent(props: {
    heading: string;
    data: any[];
    isLoading: boolean;
    children?: React.ReactNode;
}) {
    return (
        <div className="mt-20">
            <div>
                <p className="pb-4 text-2xl uppercase">{props.heading}</p>
            </div>
            {props.children}
            <Carousel className="w-full">
                <CarouselPrevious />
                <CarouselNext />
                <CarouselContent>
                    {props.isLoading ? (
                        Array(6)
                            .fill(0)
                            .map((_, index) => (
                                <CarouselItem
                                    key={index}
                                    className="basis-10/12 text-xs sm:basis-1/2 md:basis-1/4 xl:basis-1/6"
                                >
                                    <Skeleton className="aspect-[3/4]" />
                                </CarouselItem>
                            ))
                    ) : !props.data?.length ? (
                        <div className="flex h-[280px] w-full flex-col items-center justify-center gap-2">
                            <BookOpenText size={48} />
                            <p className="text-muted-foreground">Không có truyện nào, hãy quay lại vào ngày mai!</p>
                        </div>
                    ) : (
                        props.data?.map((item, i) => (
                            <CarouselItem
                                key={i}
                                className="group basis-10/12 text-xs sm:basis-1/2 md:basis-1/4 xl:basis-1/6"
                            >
                                <SeriesCard data={item} key={i} />
                            </CarouselItem>
                        ))
                    )}
                </CarouselContent>
            </Carousel>
        </div>
    );
}
