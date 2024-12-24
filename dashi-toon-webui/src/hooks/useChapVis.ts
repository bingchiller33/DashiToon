import { getUserAccessibleChapters } from "@/utils/api/series";
import { useRouter } from "next/navigation";
import { useEffect, useState } from "react";
import { toast } from "sonner";

export function useChapVis(seriesId: string, chapterId: string, isPreview: boolean) {
    const router = useRouter();
    const [isChecking, setIsChecking] = useState(true);

    useEffect(() => {
        let mounted = true;

        async function work() {
            if (isPreview) {
                return;
            }

            try {
                setIsChecking(true);
                const [vc, vcerr] = await getUserAccessibleChapters(seriesId);

                if (!mounted) return;

                if (vcerr) {
                    toast.error("Không thể tải được tập truyện");
                    return;
                }

                const isAllowed = vc.data.flatMap((x) => x.chapters).find((x) => x.id.toString() === chapterId);

                if (!isAllowed) {
                    router.replace(`/series/${seriesId}`);
                }
            } finally {
                if (mounted) {
                    setIsChecking(false);
                }
            }
        }

        work();

        return () => {
            mounted = false;
        };
    }, [seriesId, chapterId, router, isPreview]);

    return isChecking;
}
