import React from "react";
import { Carousel, CarouselContent, CarouselItem, CarouselNext, CarouselPrevious } from "@/components/ui/carousel";
import { Skeleton } from "@/components/ui/skeleton";
import SeriesContinueCard from "@/components/SeriesContinueCard";

export default function ContinueCarousel(props: {
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
                    {props.isLoading
                        ? Array(6)
                              .fill(0)
                              .map((_, index) => (
                                  <CarouselItem
                                      key={index}
                                      className="basis-10/12 text-xs sm:basis-1/2 md:basis-1/4 xl:basis-1/6"
                                  >
                                      <Skeleton className="aspect-[3/4]" />
                                  </CarouselItem>
                              ))
                        : props?.data?.map((item, i) => (
                              <CarouselItem
                                  key={i}
                                  className="group basis-10/12 text-xs sm:basis-1/2 md:basis-1/4 xl:basis-1/6"
                              >
                                  <SeriesContinueCard data={item} key={i} />
                              </CarouselItem>
                          ))}
                </CarouselContent>
            </Carousel>
        </div>
    );
}
