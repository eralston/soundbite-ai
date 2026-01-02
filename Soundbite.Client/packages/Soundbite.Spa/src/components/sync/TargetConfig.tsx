import classnames from "classnames";
import { observer } from "mobx-react-lite";
import React, { useEffect, useRef, useState } from "react";
import { Nav, NavItem, NavLink, TabContent, TabPane } from "reactstrap";

import {
  CollectionSyncMode,
  GroupSyncConfig,
  MemberSyncOpType,
  PublicError,
  SyncService,
  SyncStrategyConfigBase,
  SyncTarget,
  TokenPageRequest,
} from "@soundbite/api";
import { Loader, ShowWhen } from "@soundbite/widgets-react";

import { SyncType } from "../../modules/Sync";
import { TargetCache } from "./TargetCache";
import { TargetPicker } from "./TargetPicker";

export enum TargetConfigMode {
  Users,
  Groups,
}

interface IProps {
  children?: React.ReactNode;
  defaultConfig?: object;
  mode: TargetConfigMode;
  onChange: (newConfig: SyncStrategyConfigBase) => void;
  orgRoute: string;
  syncType: SyncType;
}

/** React component generalizing the styles of picking sync targets across none, selected, and all */
export const TargetConfig: React.FC<IProps> = observer((props: IProps) => {
  const [selectedTargets, setSelectedTargets] = useState<
    SyncTarget[] | undefined
  >(undefined);

  const setGroups = async (targets: SyncTarget[]): Promise<void> => {
    setSelectedTargets([...targets]);
  };

  const propsConfig = props.defaultConfig as SyncStrategyConfigBase | undefined;

  const targetName = props.mode === TargetConfigMode.Groups ? "Group" : "User";

  const targets =
    props.mode === TargetConfigMode.Groups
      ? propsConfig?.groups?.map((g) => {
          return { id: g.groupId };
        })
      : propsConfig?.users?.map((u) => {
          return { id: u.id };
        });
  const method =
    props.mode === TargetConfigMode.Groups
      ? SyncService.readGroupAsync
      : SyncService.readUserAsync;
  const cache = useRef(
    new TargetCache(
      props.orgRoute,
      setGroups,
      method,
      (id: string, err: Error) => {},
      targets ?? []
    )
  );

  const [collectionMode, setCollectionMode] = useState<
    CollectionSyncMode | undefined
  >(undefined);

  useEffect(() => {
    cache.current.load();
    const startingCollectionMode =
      props.mode === TargetConfigMode.Groups
        ? propsConfig?.groupsMode
        : propsConfig?.usersMode;
    setCollectionMode(startingCollectionMode ?? CollectionSyncMode.None);
    // eslint-disable-next-line
  }, []);

  const newGroups = (targets?: SyncTarget[]): GroupSyncConfig[] | undefined => {
    return targets?.map((g): GroupSyncConfig => {
      return {
        groupId: g.id,
        memberSyncType: MemberSyncOpType.SyncAllMembers,
      };
    });
  };

  const onChangeTab = (newMode: CollectionSyncMode): void => {
    setCollectionMode(newMode);

    function createNewConfig(): SyncStrategyConfigBase {
      if (props.mode === TargetConfigMode.Groups) {
        const ret: SyncStrategyConfigBase = {
          groupsMode: newMode,
          groups: newGroups(selectedTargets),
          syncType: props.syncType,
        };
        return ret;
      } else if (props.mode === TargetConfigMode.Users) {
        const ret: SyncStrategyConfigBase = {
          usersMode: newMode,
          users: selectedTargets,
          syncType: props.syncType,
        };
        return ret;
      } else {
        throw new PublicError(
          undefined,
          "Cannot change collection mode; Unknown Target Config Mode"
        );
      }
    }

    const newConfig = createNewConfig();
    props.onChange(newConfig);
  };

  const onChangeTargets = (newTargets: SyncTarget[]) => {
    setSelectedTargets(newTargets);

    function createNewConfig(): SyncStrategyConfigBase {
      if (props.mode === TargetConfigMode.Groups) {
        const ret: SyncStrategyConfigBase = {
          groupsMode: collectionMode,
          groups: newGroups(newTargets),
          syncType: props.syncType,
        };
        return ret;
      } else if (props.mode === TargetConfigMode.Users) {
        const ret: SyncStrategyConfigBase = {
          usersMode: collectionMode,
          users: newTargets,
          syncType: props.syncType,
        };
        return ret;
      } else {
        throw new PublicError(
          undefined,
          "Cannot Change Targets; Unknown Target Config Mode"
        );
      }
    }
    const newConfig = createNewConfig();
    props.onChange(newConfig);
  };

  const loadPage = (page?: TokenPageRequest) => {
    if (props.mode === TargetConfigMode.Groups) {
      return SyncService.readGroupsAsync(props.orgRoute, page);
    } else if (props.mode === TargetConfigMode.Users) {
      return SyncService.readUsersAsync(props.orgRoute, page);
    } else {
      throw new PublicError(
        undefined,
        "Cannot Load Targets; Unknown Target Config Mode"
      );
    }
  };

  const SomeTab: React.FC = () => {
    return (
      <div>
        <div className="mb-2">
          <ShowWhen is={props.mode === TargetConfigMode.Groups}>
            Users in the following groups will be synchronized:
          </ShowWhen>
          <ShowWhen is={props.mode === TargetConfigMode.Users}>
            Users on the following list will be synchronized:
          </ShowWhen>
        </div>
        <Loader isLoadedWhen={collectionMode === CollectionSyncMode.Some}>
          <TargetPicker
            onChange={onChangeTargets}
            onLoad={loadPage}
            selected={selectedTargets}
            title={`${targetName}s`}
          />
        </Loader>
      </div>
    );
  };

  const createTab = (
    title: string,
    value: CollectionSyncMode,
    onClick: () => void,
    currentMode: CollectionSyncMode
  ) => {
    const className = classnames(
      { active: value === currentMode },
      "sb-tab-btn"
    );

    return (
      <NavItem>
        <NavLink
          role="tab"
          aria-selected={value === currentMode}
          className={className}
          onClick={onClick}
        >
          {title}
        </NavLink>
      </NavItem>
    );
  };

  const tabs = () => {
    return (
      <Loader isLoadedWhen={collectionMode != null}>
        <div className="d-flex flex-md-row flex-row mt-2">
          <Nav className="sb-3-tab flex-column flex-md-row" role="tablist">
            {createTab(
              `No ${targetName}s`,
              CollectionSyncMode.None,
              () => {
                onChangeTab(CollectionSyncMode.None);
              },
              collectionMode as CollectionSyncMode
            )}
            {createTab(
              `Selected ${targetName}s`,
              CollectionSyncMode.Some,
              () => {
                onChangeTab(CollectionSyncMode.Some);
              },
              collectionMode as CollectionSyncMode
            )}
            {createTab(
              `All ${targetName}s`,
              CollectionSyncMode.All,
              () => {
                onChangeTab(CollectionSyncMode.All);
              },
              collectionMode as CollectionSyncMode
            )}
          </Nav>
        </div>
        <TabContent activeTab={collectionMode} className="ml-3 mb-4">
          <TabPane tabId={CollectionSyncMode.None}>
            {targetName}s will <strong>not</strong> be synced. They can still be
            managed manually in Soundbite.
          </TabPane>
          <TabPane tabId={CollectionSyncMode.Some}>
            <SomeTab />
          </TabPane>
          <TabPane tabId={CollectionSyncMode.All}>
            Synchronize all {targetName}s in your organization
            <ShowWhen is={props.mode === TargetConfigMode.Groups}>
              <div>
                <strong>Not recommended for large organizations</strong>
              </div>
            </ShowWhen>
          </TabPane>
        </TabContent>
      </Loader>
    );
  };

  return (
    <div>
      {props.children}
      {tabs()}
    </div>
  );
});
