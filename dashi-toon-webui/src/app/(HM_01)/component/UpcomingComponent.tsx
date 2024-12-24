/* eslint-disable @next/next/no-img-element */
import { Badge } from "@/components/ui/badge";
import { Card, CardContent } from "@/components/ui/card";
import {
    Carousel,
    CarouselContent,
    CarouselItem,
    CarouselNext,
    CarouselPrevious,
} from "@/components/ui/carousel";
import Link from "next/link";
import CountdownTimer from "./CountDown";

export default function UpcomingComponent(props: {
    heading: string;
    data: any[];
    isLoading: boolean;
}) {
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
                        // eslint-disable-next-line react/jsx-key
                        <CarouselItem className="basis-1/2 text-xs lg:basis-1/5">
                            <Link href={`/series/${item.seriesId}`}>
                                <Card>
                                    <div className="relative">
                                        <img
                                            className="w-full rounded-t-lg"
                                            style={{ aspectRatio: 266 / 350 }}
                                            src={item.thumbnail}
                                            alt=""
                                        />
                                        <div className="absolute left-0 top-0 rounded-tl-lg bg-slate-700 p-1">
                                            {item.type}
                                        </div>

                                        <CountdownTimer
                                            leftTime={item?.leftTime}
                                        />
                                    </div>
                                    <CardContent className="px-1 py-2">
                                        <div className="flex items-center justify-between">
                                            <div>{item.timeAgo}</div>
                                        </div>
                                        <div className="text-sm">
                                            {item.title}
                                        </div>
                                    </CardContent>
                                </Card>
                            </Link>
                        </CarouselItem>
                    ))}
                </CarouselContent>
            </Carousel>
        </div>
    );
}
