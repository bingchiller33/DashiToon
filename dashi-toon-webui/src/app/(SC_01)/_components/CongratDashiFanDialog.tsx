"use client";

import { Button } from "@/components/ui/button";
import { Dialog, DialogContent, DialogDescription, DialogHeader, DialogTitle } from "@/components/ui/dialog";
import { DashiFanTier, getDashiFanTiers, SeriesResp } from "@/utils/api/series";
import { getActiveSub, Subscription2 } from "@/utils/api/subscription";
import { Zap } from "lucide-react";
import { useEffect, useState } from "react";
import { PiKeyReturnLight } from "react-icons/pi";

export interface DashiFanSectionProps {
    seriesId: string;
}

export interface DashiFanSectionProps {
    seriesId: string;
    tierId?: string;
    defaultOpen: boolean;
}

export default function CongratDashiFanDialog(props: DashiFanSectionProps) {
    const { seriesId, defaultOpen, tierId } = props;

    const [showCongrats, setShowCongrats] = useState(defaultOpen);
    const [currTier, setCurrTier] = useState<DashiFanTier | null>();
    const [seriesInfo, setSeriesInfo] = useState<SeriesResp>();

    useEffect(() => {
        async function work() {
            if (!tierId) return;
            const [data, err] = await getDashiFanTiers(seriesId);
            console.log({ tierId });
            if (err) {
                setShowCongrats(false);
                return;
            }

            setCurrTier(data.find((x) => x.id === tierId) || null);
        }

        work();
    }, [seriesId, tierId]);

    return (
        <Dialog open={showCongrats} onOpenChange={setShowCongrats}>
            <DialogContent className="border-none bg-gradient-to-b from-neutral-800 to-neutral-900 text-white">
                <DialogHeader>
                    <DialogTitle className="text-center text-3xl font-bold">Chúc mừng!</DialogTitle>
                    <DialogDescription className="text-center text-xl text-gray-100">
                        Chúc mừng, Bạn đã trở thành DashiFan
                    </DialogDescription>
                </DialogHeader>
                <div className="my-6 flex justify-center">
                    <Zap className="h-24 w-24 text-indigo-400" size={100} />
                </div>
                <p className="text-center text-lg">
                    Chào mừng đến với hạng <span className="font-semibold text-blue-400">{currTier?.name}</span>
                </p>
                <p className="text-center">
                    Bạn có thể đọc trước <span className="font-bold">{currTier?.perks} chương truyện</span>!
                </p>
                <p className="text-center text-muted-foreground">Sẽ mất vài giây để hệ thống xử lý đơn hàng của bạn.</p>
                <div className="mt-6 flex justify-center">
                    <Button className="bg-white text-blue-600 hover:bg-gray-100" onClick={() => setShowCongrats(false)}>
                        Bắt đầu đọc ngay!
                    </Button>
                </div>
            </DialogContent>
        </Dialog>
    );
}
