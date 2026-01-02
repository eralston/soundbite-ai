import { PeopleService, Person, PersonRole, Utils } from "@soundbite/api";
import React, { useEffect, useState } from "react";
import { Button } from "reactstrap";
import { WidgetStore } from "../store";
import { Avatar, Loader, ShowWhen } from "./controls";
import { ErrorDlg } from "./ErrorDlg";

/***************************************************************************************************
 *  Component Properties Interface
 **************************************************************************************************/
interface IProps {
  className?: string;
  orgRoute?: string;
  title: string;
  roleMsg: string;
}

/**
 * Provides a standard way of creating buttons in Soundbite Studio.
 */
export const AskAccess: React.FC<IProps> = (props: IProps) => {
  const [isLoading, setIsLoading] = useState(true);
  const [admins, setAdmins] = useState<Person[] | undefined>();

  const actionMsg = `Request ${props.roleMsg} Access`;

  useEffect(() => {
    const load = async () => {
      try {
        setIsLoading(true);

        // If we don't have an orgRoute yet, then abort
        if (props.orgRoute == null) {
          return;
        }

        const response = await PeopleService.readAllAsync(props.orgRoute, {
          personRole: PersonRole.Admin,
        });
        setAdmins(response.result);
      } catch (err) {
        ErrorDlg.show(err, "Error Retrieving List of Admins");
      } finally {
        setIsLoading(false);
      }
    };
    load();
  }, [props.orgRoute]);

  function onRequestAccess(person: Person) {
    let email = person.user.email;

    if (person.user.givenName != null) {
      const receiverName = Utils.userDisplay(person.user);
      email = `${receiverName}<${person.user.email}>`;
    }

    let subject = actionMsg;
    const currentUser = WidgetStore.organizations?.currentOrg?.details.me;

    if (currentUser != null) {
      const currentUserName = Utils.userDisplay(currentUser?.user);
      subject = subject + " for " + currentUserName;
    }

    let body = "";

    if (person.user.givenName != null) {
      body += `${person.user.givenName},\r\n\r\n`;
    }

    body += `I'm reaching out to request access to create content in Soundbite. I can be added at the organization level or scoped just to a group. Let me know what you need from me to start creating content for my team?\r\n\r\n`;
    const supportEmail = "Support@Soundbite.Freshdesk.Com";
    body += `For more information on how organization and group permissions work in Soundbite, contact Soundbite Support: ${supportEmail}`;
    const mailtoLink = `mailto:${email}?subject=${encodeURIComponent(
      subject
    )}&body=${encodeURIComponent(body)}`;

    // Create a link element, set the href, and click it programmatically
    const link = document.createElement("a");
    link.href = mailtoLink;
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
  }

  //////////[ Build Component UI ]//////////////////////////////////////////////////////////////////

  const hasMoreThanOneAdmin = (admins?.length ?? 0) > 1;
  return (
    <div className={`border-0 ${props.className ?? ""}`}>
      <Loader isLoadedWhen={props.orgRoute != null && !isLoading}>
        <p className="align-items-center text-muted">{props.title}</p>
        <table className="table align-items-center">
          <thead className="thead-light">
            <tr>
              <th scope="col" className="sort" data-sort="name">
                Admin
              </th>
              <th scope="col" className="sort" data-sort="action"></th>
            </tr>
          </thead>
          <tbody>
            {admins?.map((person) => {
              return (
                <tr key={person.route}>
                  <th scope="row">
                    <Avatar
                      user={person.user}
                      onClick={() => onRequestAccess(person)}
                      title="Show Member Profile"
                    />
                  </th>
                  <td className="text-right">
                    <ShowWhen is={hasMoreThanOneAdmin}>
                      <Button
                        onClick={() => onRequestAccess(person)}
                        color="primary"
                        size={"sm"}
                        type="button"
                        title={actionMsg}
                      >
                        {actionMsg}
                      </Button>
                    </ShowWhen>
                  </td>
                </tr>
              );
            })}
          </tbody>
        </table>
      </Loader>
    </div>
  );
};
