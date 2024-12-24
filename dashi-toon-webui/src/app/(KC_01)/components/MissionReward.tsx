import { useState, useEffect } from "react";
import { Button } from "@/components/ui/button";
import { Progress } from "@/components/ui/progress";
import { Coins } from "lucide-react";
import {
    claimMissionReward,
    getUserMetadata,
    getUserMissions,
    Mission,
    UserMetadata,
} from "@/utils/api/reader/kana";
import { toast } from "sonner";

type MissionRewardsProps = {
    onClaimReward: () => void;
};

export function MissionRewards({ onClaimReward }: MissionRewardsProps) {
    const [missions, setMissions] = useState<Mission[]>([]);
    const [userMetadata, setUserMetadata] = useState<UserMetadata | null>(null);

    const fetchMissions = async () => {
        const [data, error] = await getUserMissions();
        if (data) {
            setMissions(data);
        } else {
            toast.error("Không thể tải nhiệm vụ", error);
        }
    };

    const fetchUserMetadata = async () => {
        const [data, error] = await getUserMetadata();
        if (data) {
            setUserMetadata(data);
        } else {
            toast.error("Không thể tải thông tin người dùng", error);
        }
    };

    useEffect(() => {
        fetchMissions();
        fetchUserMetadata();
    }, []);

    const handleClaimReward = async (missionId: string) => {
        const [data, error] = await claimMissionReward(missionId);
        if (error) {
            toast.error("Không thể nhận thưởng", error);
        } else {
            toast.success("Đã nhận thưởng thành công");
            fetchMissions();
            onClaimReward();
        }
    };

    return (
        <div className="space-y-4">
            <div className="flex flex-col space-y-1.5">
                <div className="flex items-center justify-between">
                    <h3 className="text-xl font-semibold tracking-tight text-gray-200">
                        Nhiệm Vụ
                    </h3>
                </div>

                <p className="text-sm text-muted-foreground">
                    Làm nhiệm vụ để nhận Kana Coin.
                </p>
            </div>
            {missions.map((mission) => {
                const progress = Math.min(
                    ((userMetadata?.currentDateChapterRead ?? 0) /
                        mission.readCount) *
                        100,
                    100,
                );
                return (
                    <div
                        key={mission.missionId}
                        className="space-y-2 rounded-lg border border-neutral-700/50 bg-neutral-800/70 p-4 shadow-md shadow-black/30 transition-all duration-300 hover:border-neutral-600"
                    >
                        <div className="flex items-center justify-between">
                            <span className="text-sm text-gray-300">
                                Đọc {mission.readCount} chương mới
                            </span>
                            <span className="flex items-center text-sm text-gray-200">
                                <Coins className="mr-2 h-6 w-6 flex-shrink-0 text-gray-400" />
                                {mission.amount} Kana Coin
                            </span>
                        </div>
                        <Progress
                            value={progress}
                            className="h-2 transition-all duration-500 ease-in-out"
                        />
                        <div className="flex items-center justify-between">
                            <span className="text-xs text-gray-400">
                                {userMetadata?.currentDateChapterRead ?? 0}/
                                {mission.readCount}
                            </span>
                            <Button
                                variant="outline"
                                size="sm"
                                disabled={
                                    !mission.isCompletable ||
                                    mission.isCompleted
                                }
                                onClick={() =>
                                    handleClaimReward(mission.missionId)
                                }
                                className="border-blue-300 text-blue-300 hover:bg-blue-300 hover:text-neutral-800"
                            >
                                {mission.isCompleted
                                    ? "Đã nhận"
                                    : "Nhận thưởng"}
                            </Button>
                        </div>
                    </div>
                );
            })}
        </div>
    );
}
