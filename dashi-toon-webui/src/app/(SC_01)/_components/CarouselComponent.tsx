import React from "react";
import { Carousel, CarouselContent, CarouselItem, CarouselNext, CarouselPrevious } from "@/components/ui/carousel";
import { RelatedSeries } from "@/utils/api/series";
import SeriesCard from "@/components/SeriesCard";

export default function CarouselComponent(props: { heading: string; data: RelatedSeries[] }) {
    return (
        <div className="my-20">
            <div>
                <p className="text-2xl uppercase">{props.heading}</p>
            </div>
            <Carousel className="w-full">
                <CarouselPrevious />
                <CarouselNext />
                <CarouselContent>
                    {props?.data?.map((item, index) => (
                        <CarouselItem key={index} className="group basis-10/12 text-xs sm:basis-1/2 md:basis-1/4">
                            <SeriesCard data={item as any} />
                        </CarouselItem>
                    ))}
                </CarouselContent>
            </Carousel>
        </div>
    );
}
