import React, { Fragment, useRef, useState } from "react";
import { observer } from "mobx-react-lite";
import { Card, CardHeader, Row, Col, Button } from "reactstrap";

import {
  IndexPageResponse,
  Invite,
  OrganizationWithPermissions,
  PageUtils,
  Person,
  PersonRole,
} from "@soundbite/api";

import { WidgetStore } from "../store";
import { DeleteUserDialog, DialogManager, InviteUserDialog } from "../dialogs";
import { Avatar, ShowWhen, Toggle } from "../components/controls";
import { ErrorDlg, Loader } from "../components";
import { InfiniteTable } from "../components/controls/InfiniteTable";
import { InfiniteScrollDataHelper } from "../modules/InfiniteScrollDataHelper";

interface IProps {
  orgRoute: string;
}

/** Display the list of people in the given org */
export const PeopleWidget: React.FC<IProps> = observer((props: IProps) => {
  let { orgRoute } = props;

  // Component state
  const [isLoading, setIsLoading] = useState(false);
  const [org, setOrg] = useState<OrganizationWithPermissions | undefined>();
  const myPersonRoute = org?.details.me?.route;
  const myPersonRole = org?.details.me?.personRole;

  // Dynamic People data
  const people = useRef<Map<number, Person | null>>(
    new Map<number, Person | null>()
  );
  const maxTake = useRef<number>(128);
  const maxTotalItems = useRef<number>(2147483647);
  const [count, setCount] = useState<number | undefined>(undefined);

  // Load

  const load = async (allowCache = true): Promise<void> => {
    if (isLoading || orgRoute == null) {
      return;
    }

    setIsLoading(true);
    if (!allowCache) {
      setOrg(undefined);
    }

    const firstPeoplePromise = WidgetStore.people.readAllAsync(orgRoute, {
      includesCounts: true,
    });

    const newOrgPromise = WidgetStore.organizations.readOrgAsync(
      orgRoute,
      allowCache
    );
    const [firstPeoplePage, newOrg] = await Promise.all([
      firstPeoplePromise,
      newOrgPromise,
    ]);
    setPeople(firstPeoplePage);
    setCount(firstPeoplePage.totalCount);
    maxTake.current = firstPeoplePage.maxTake;
    setOrg(newOrg);
    setIsLoading(false);
  };

  const setPeople = (page: IndexPageResponse<Person>) => {
    PageUtils.addPageToMap(page, people.current);
  };

  const scrollData: InfiniteScrollDataHelper = new InfiniteScrollDataHelper(
    async (startIndex: number, stopIndex: number): Promise<void> => {
      return new Promise<void>(async (resolve, reject) => {
        try {
          // Indicate
          for (let index = startIndex; index <= stopIndex; index++) {
            people.current.set(index, null);
          }

          const page = await WidgetStore.people.readAllAsync(orgRoute, {
            skip: startIndex - 1,
            take: maxTake.current,
          });
          PageUtils.addPageToMap(page, people.current);

          resolve();
        } catch (err) {
          ErrorDlg.show(
            err,
            "Error Loading Sessions",
            `Error querying for sessions ${startIndex} to ${stopIndex}:`
          );
          reject();
        }
      });
    }
  );

  React.useEffect(() => {
    load();
    // eslint-disable-next-line
  }, [orgRoute, WidgetStore.organizations.currentOrg]);

  const onSubmitInviteUser = async (invite?: Invite[]) => {
    if (!invite) return;

    await WidgetStore.people.invitePersonAsync(orgRoute, invite);
    await load();
  };

  // Events

  const onInvitePerson = () => {
    InviteUserDialog.open(undefined, onSubmitInviteUser);
  };

  const onSubmitDeleteUser = async (person?: Person) => {
    if (person) {
      await WidgetStore.people.deletePersonAsync(orgRoute, person.route);
      await load();
    }
  };

  const onDeleteUser = (person: Person) => {
    DeleteUserDialog.open(person, onSubmitDeleteUser);
  };

  const onPersonClick = (person: Person) => {
    DialogManager.ShowProfileDialog(person.user, false);
  };

  const onToggleAdmin = (adminChecked: boolean, person: Person) => {
    person.personRole = adminChecked ? PersonRole.Admin : PersonRole.Person;
    WidgetStore.people.updatePersonAsync(orgRoute, person);
    // No need to update because the UI already changes when the click happens
  };

  const onRefresh = async () => {
    await load(false);
  };

  // Inner Components

  const Header: React.FC = () => {
    return (
      <CardHeader className="border-0">
        <ShowWhen is={count != null}>
          <Row className="align-items-center">
            <Col>
              <h1 className="mb-0">
                <i className="fas fa-user-cog d-none d-sm-inline"></i>
                {count} {org?.details.name} Users
              </h1>
            </Col>
            <Col className="text-right">
              <Button
                outline={true}
                color="secondary"
                className="btn-icon btn-sm"
                onClick={onRefresh}
                title="Refresh Users"
              >
                <span className="btn-inner--icon">
                  <i className="fas fa-sync-alt"></i>
                </span>
              </Button>
              <button
                className="btn btn-icon btn-primary btn-sm"
                type="button"
                onClick={onInvitePerson}
                title="Invite User"
              >
                <span className="btn-inner--icon">
                  <i className="fas fa-user-plus"></i>
                </span>
                <span className="btn-inner--text">
                  <span className="d-none d-sm-inline">
                    <Fragment>&nbsp;</Fragment>Invite
                  </span>
                  <span className="d-none d-md-inline">
                    <Fragment>&nbsp;</Fragment>User
                  </span>
                </span>
              </button>
            </Col>
          </Row>
        </ShowWhen>
      </CardHeader>
    );
  };

  const TableHeader: React.FC = () => {
    return (
      <thead className="thead-light">
        <tr>
          <th scope="col" className="sort" data-sort="name">
            Name
          </th>
          <th
            scope="col"
            className="sort d-none d-lg-table-cell"
            data-sort="role"
          >
            E-Mail
          </th>
          <th scope="col" className="sort" data-sort="deadline">
            Admin
          </th>
          <th scope="col" className="sort" data-sort="action"></th>
        </tr>
      </thead>
    );
  };

  const PersonRow: React.FC<{ person: Person; index: number }> = (props: {
    person: Person;
    index: number;
  }) => {
    const person = props.person;
    return (
      <tr
        data-person-route={person.route}
        data-row-index={props.index}
        data-user-route={person.user.route}
      >
        <th scope="row">
          <Avatar
            user={person.user}
            onClick={() => onPersonClick(person)}
            title="Show User Profile"
          />
        </th>
        <td className="d-none d-lg-table-cell">
          <a
            href={`mailto:${person.user.email}`}
            title={`Send e-mail to ${person.user.email}`}
          >
            {person.user.email}
          </a>
        </td>
        <td>
          <ShowWhen is={myPersonRoute !== person.route}>
            <Toggle
              defaultValue={person.personRole === PersonRole.Admin}
              onToggle={(isEnabled: boolean) =>
                onToggleAdmin(isEnabled, person)
              }
              title="User Is Organization Administrator"
            />
          </ShowWhen>
          <ShowWhen
            is={
              myPersonRoute === person.route &&
              myPersonRole === PersonRole.Admin
            }
          >
            Admin
          </ShowWhen>
        </td>
        <td className="text-right">
          <ShowWhen is={myPersonRoute !== person.route}>
            <button
              className="btn btn-outline-secondary btn-sm"
              type="button"
              onClick={() => onDeleteUser(person)}
              title="Delete User"
            >
              <span className="btn-inner--icon">
                <i className="fas fa-trash"></i>
              </span>
            </button>
          </ShowWhen>
        </td>
      </tr>
    );
  };

  const LoadingRow: React.FC<{ index?: number }> = (props: {
    index?: number;
  }) => {
    return (
      <tr data-row-index={props.index} style={{ height: "73px" }}>
        <th scope="row" className="text-muted">
          Loading {props.index != null ? `${props.index}...` : "..."}
        </th>
        <td className="d-none d-lg-table-cell"></td>
        <td></td>
        <td></td>
      </tr>
    );
  };

  const LastRow: React.FC = () => {
    return (
      <tr style={{ height: "73px" }}>
        <th scope="row" className="text-muted">
          Cannot load more than {maxTotalItems.current} at this time
        </th>
        <td className="d-none d-lg-table-cell"></td>
        <td></td>
        <td></td>
      </tr>
    );
  };

  /** The Row component. This should be a table row, and noted that we don't use the style that regular `react-window` examples pass in.*/
  function TableRow({ index }: { index: number }) {
    const person = people.current.get(index);
    if (index >= maxTotalItems.current) {
      return <LastRow />;
    } else if (person == null) {
      return <LoadingRow index={index} />;
    } else {
      return <PersonRow person={person} index={index} />;
    }
  }

  const Table: React.FC = () => {
    // Every row is loaded except for our loading indicator row.
    const isItemLoaded = (index: number) => {
      const person = people.current.get(index);
      return person != null;
    };

    let itemCount = count ?? 0;
    if (itemCount > maxTotalItems.current) {
      itemCount = maxTotalItems.current + 1;
    }

    return (
      <ShowWhen is={count != null}>
        <InfiniteTable
          count={itemCount}
          header={<TableHeader />}
          isItemLoaded={isItemLoaded}
          itemSize={73}
          loadMore={(startIndex: number, endIndex: number) =>
            scrollData.getMoreItems(startIndex, endIndex)
          }
          minimumBatchSize={128}
          row={TableRow}
        />
      </ShowWhen>
    );
  };

  const Body: React.FC = () => {
    return (
      <div className="table-responsive">
        <ShowWhen is={!!org}>
          <Table />
        </ShowWhen>
      </div>
    );
  };

  return (
    <Card>
      <Loader
        isLoadedWhen={!isLoading && !!org && people != null}
        style={{ minHeight: "12rem" }}
        message="Loading Users..."
      >
        <Header />
        <Body />
      </Loader>
    </Card>
  );
});
