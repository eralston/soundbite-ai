import React, { useState } from "react";
import { observer } from "mobx-react-lite";
import {
  Button,
  FormGroup,
  InputGroup,
  InputGroupAddon,
  InputGroupText,
} from "reactstrap";
import { getTenantId } from "../../../sdk/providers/auth/AadAuthProvider";

interface IAadTenantPickerProps {
  defaultTenantId?: string;
  onChange?: (newTenantId?: string) => void;
}

export const AadTenantPicker: React.FC<IAadTenantPickerProps> = observer(
  (props: IAadTenantPickerProps) => {
    const changeTenantId = (newTenantId?: string) => {
      setTenantId(newTenantId);
      if (props.onChange) props.onChange(newTenantId);
    };

    const onTenantIdChange = async (
      event: React.ChangeEvent<HTMLInputElement>
    ) => {
      const newTenantId = event.target.value;
      changeTenantId(newTenantId);
    };

    const [tenantId, setTenantId] = useState(props.defaultTenantId);

    const onFind = async () => {
      const newTenantId = await getTenantId();
      changeTenantId(newTenantId);
    };

    return (
      <div className="sb-tenant-picker">
        <FormGroup>
          <InputGroup>
            <InputGroupAddon addonType="prepend">
              <span className="input-group-text">
                <i className="fab fa-microsoft" />
              </span>
              <InputGroupText className="ml-0 pl-0">Tenant ID</InputGroupText>
            </InputGroupAddon>
            <input
              type="text"
              className="form-control"
              placeholder="Active Directory Tenant ID"
              aria-label="Active Directory Tenant ID"
              value={tenantId ?? ""}
              onChange={onTenantIdChange}
              title="Tenant ID"
            />
            <InputGroupAddon addonType="append">
              <InputGroupText>
                <Button
                  onClick={onFind}
                  color="secondary"
                  outline={true}
                  className="btn-sm"
                  title="Find My Tenant ID"
                >
                  Find My Tenant ID
                </Button>
              </InputGroupText>
            </InputGroupAddon>
          </InputGroup>
        </FormGroup>
      </div>
    );
  }
);
