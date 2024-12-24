// import { useState, useEffect } from "react";
// import { Button } from "@/components/ui/button";
// import { DropdownMenuItem } from "@/components/ui/dropdown-menu";
// import { Coins } from "lucide-react";
// import { DailyCheckIn } from "./daily-check-in";
// import { toast } from "sonner";

// interface CurrencyActivityProps {
//     session: any | null;
//     isMobile?: boolean;
// }

// export function CurrencyDropdown({
//     session,
//     isMobile = false,
// }: CurrencyActivityProps) {
//     const [kanaCoin, setKanaCoin] = useState(0);
//     const [isCheckedIn, setIsCheckedIn] = useState(false);

//     useEffect(() => {
//         if (session) {
//             fetchKanaCoinBalance();
//             checkDailyClaimStatus();
//         }
//     }, [session]);

//     useEffect(() => {
//         const fetchData = async () => {
//             setIsLoading(true);
//             await Promise.all([fetchUserKanas(), fetchUserMetadata()]);
//             setIsLoading(false);
//         };
//         fetchData();
//     }, []);

//     const fetchUserKanas = async () => {
//         const [data, error] = await getUserKanas();
//         if (data) {
//             setUserKanas(data);
//         } else {
//             toast.error("Lỗi khi tải thông tin Kana", error);
//         }
//     };

//     async function checkDailyClaimStatus() {
//         // Implement API call to check if daily reward has been claimed
//         // For now, we'll use a placeholder value
//         setIsCheckedIn(false);
//     }

//     const handleCheckIn = () => {
//         // This function will be called after a successful check-in
//         setIsCheckedIn(true);
//         setKanaCoin((prevCoin) => prevCoin + 100);
//         fetchKanaCoinBalance(); // Refresh the balance after check-in
//     };

//     if (!session) return null;

//     if (isMobile) {
//         return (
//             <div className="mt-4 border-t border-neutral-700 pt-4">
//                 <div className="mb-2 flex items-center justify-between px-2">
//                     <div className="flex items-center">
//                         <Coins className="mr-1 h-5 w-5 text-yellow-400" />
//                         <span>{kanaCoin} Kana Coin</span>
//                     </div>
//                 </div>
//                 <DailyCheckIn
//                     isCheckedIn={isCheckedIn}
//                     onCheckIn={handleCheckIn}
//                 />
//             </div>
//         );
//     }

//     return (
//         <>
//             <div className="mr-4 hidden items-center md:flex">
//                 <Coins className="mr-1 h-5 w-5 text-yellow-400" />
//                 <span className="mr-2 text-white">{kanaCoin}</span>
//                 <DailyCheckIn
//                     isCheckedIn={isCheckedIn}
//                     onCheckIn={handleCheckIn}
//                 />
//             </div>
//             <DropdownMenuItem className="px-2 py-1.5">
//                 <Coins className="mr-2 h-4 w-4 text-yellow-400" />
//                 <span>{kanaCoin} Kana Coin</span>
//             </DropdownMenuItem>
//             <DropdownMenuItem asChild>
//                 <div className="px-2 py-1.5">
//                     <DailyCheckIn
//                         isCheckedIn={isCheckedIn}
//                         onCheckIn={handleCheckIn}
//                     />
//                 </div>
//             </DropdownMenuItem>
//         </>
//     );
// }
