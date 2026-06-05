"use client";

import { useMemo, useState } from "react";

import { Header } from "@/components/layout/Header";
import { mockRivers } from "@/features/search/constants/mockRivers";
import { RiverList } from "@/features/search/components/RiverList";
import { RiverListSelect } from "@/features/search/components/RiverListSelect";
import { RiverListToolBar } from "@/features/search/components/RiverListToolBar";
import { Footer } from "@/components/layout/Footer";

export default function WaterBodies() {
  const [selectedRiverId, setSelectedRiverId] = useState<string | undefined>();

  const visibleRivers = useMemo(() => {
    if (!selectedRiverId) {
      return mockRivers;
    }

    const selectedRiver = mockRivers.find(
      (river) => river.id === selectedRiverId
    );

    return selectedRiver ? [selectedRiver] : mockRivers;
  }, [selectedRiverId]);

  return (
    <div className="min-h-screen bg-background text-text-primary">
      <Header />

      <main className="mx-auto w-full max-w-7xl px-3 py-5 sm:px-6 sm:py-6 lg:px-8 pb-4">
        <RiverListToolBar />

        <section className="mt-6 grid gap-5 sm:mt-8 sm:gap-7 lg:grid-cols-[minmax(250px,300px)_minmax(0,1fr)] lg:items-start xl:gap-14">
          <aside className="flex justify-center lg:sticky lg:top-24 lg:block">
            <RiverListSelect
              selectedRiverId={selectedRiverId}
              onSelectRiver={(river) =>
                setSelectedRiverId((currentRiverId) =>
                  currentRiverId === river.id ? undefined : river.id
                )
              }
              className="max-w-full sm:max-w-[360px] lg:max-w-none"
            />
          </aside>

          <RiverList
            rivers={visibleRivers}
            selectedRiverId={selectedRiverId}
            className="mx-auto max-w-[760px] lg:mx-0 lg:max-h-[calc(100vh-220px)] lg:max-w-none lg:overflow-y-auto lg:overscroll-contain lg:pr-3 lg:[-ms-overflow-style:none] lg:[scrollbar-width:none] lg:[&::-webkit-scrollbar]:hidden"
          />
        </section>
      </main>
      <Footer />
    </div>
  );
}
